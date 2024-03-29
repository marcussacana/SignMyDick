﻿using System;
using Iced.Intel;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using static SignMyDick.Hook.Extensions;

namespace SignMyDick.Hook
{
    public abstract unsafe class Hook<T> : Hook where T : Delegate
    {
        public Hook() : base() { }
        public Hook(void* Function) : base(Function) { }

        private UnsafeDelegate HookInstance;
        private UnsafeDelegate<T> BypassInstance;

        public T Bypass
        {
            get
            {
                if (BypassInstance == null)
                    BypassInstance = BypassFunction;

                return BypassInstance;
            }
        }

        public virtual T HookDelegate
        {
            set
            {
                HookInstance = value;
                HookFunction = HookInstance;
            }
        }
    }

    public abstract unsafe class Hook
    {
        public Hook() { Initialize(); }

        public Hook(void* Function)
        {
            this.Function = Function;
            Initialize();
        }

        ~Hook()
        {
            Uninstall();
        }

        public abstract string Library { get; }
        public abstract string Export { get; }
        public virtual ushort Ordinal => 0;

        public virtual string Name => Export;

        /// <summary>
        /// The hook function address
        /// </summary>
        public virtual void* HookFunction { get; set; }

        /// <summary>
        /// The target function Export
        /// </summary>
        public void* Function { get; private set; }

        /// <summary>
        /// The address of the bypass hook function
        /// </summary>
        public void* BypassFunction { get; private set; }

        public byte[] BypassBuffer;
        public byte[] RealBuffer;
        public byte[] HookBuffer;

        public bool Enabled { get; private set; } = false;
        public bool Persistent;

        public void Compile(bool ImportHook = false, IntPtr? TargetModule = null)
        {
            var hModule = GetLibrary(Library);

            if (hModule == null)
                throw new DllNotFoundException(Library);

            if (Export != null)
                Function = GetProcAddress(hModule, Export);
            else
                Function = GetProcAddress(hModule, Ordinal);

            if (ImportHook)
                SetupImportHook(TargetModule == null ? System.Diagnostics.Process.GetCurrentProcess().MainModule.BaseAddress.ToPointer() : TargetModule.Value.ToPointer());
            else
                AssemblyHook();        }

        public void Compile(void* Function)
        {
            this.Function = Function;
            AssemblyHook();
        }

#if x64
        uint? _JmpSize = null;
        public uint JmpSize => (_JmpSize ?? (_JmpSize = (uint)ulong.MaxValue.AssemblyJmp().GetEncodedSize(64))).Value;
        private void AssemblyHook()
        {
            //Copy Minimal Instructions Amount
            var Reader = new MemoryCodeReader(this.Function, 100);
            var Decoder = Iced.Intel.Decoder.Create(64, Reader);
            Decoder.IP = (ulong)Function;

            var Instructions = Decoder.DecodeMany(JmpSize);

            var RetAddr = Decoder.IP;

            //If the next instruction is a conditional jmp,
            //Will be more safe if we copy it to the bypass as well
            var NextInstruction = Decoder.PeekDecode();
            if (NextInstruction.IsCondJmp())
                Instructions.Add(NextInstruction);


            //Allocate Memory
            var Writer = new MemoryCodeWriter();
            var Compiler = Encoder.Create(64, Writer);

            int RHookSize = Instructions.GetEncodedSize(64, (ulong)Function);
            int BypassSize = Instructions.GetAutoEncodedSize(64, (ulong)Function);

            RealBuffer = new byte[RHookSize];
            BypassBuffer = new byte[BypassSize];

            DeprotectMemory(Function, (uint)BypassBuffer.LongLength);
            Marshal.Copy(new IntPtr(Function), RealBuffer, 0, RealBuffer.Length);


            //Assemble Bypass
            var phBuffer = BypassBuffer.AllocUnsafe();
            BypassFunction = phBuffer;

            Instructions.AddRange(RetAddr.AssemblyJmp());

            Compiler.AutoEncode(Instructions, (ulong)phBuffer);

            Writer.CopyTo(phBuffer, 0);

            //Assemble the Hook
            Writer = new MemoryCodeWriter();
            Compiler = Encoder.Create(64, Writer);

            Instructions = ((ulong)HookFunction).AssemblyJmp();

            Compiler.Encode(Instructions, (ulong)Function);

            HookBuffer = Writer.ToArray();
        }
#else
        public const int JmpSize = 5;
        private void AssemblyHook()
        {
            //Copy Minimal Instructions Amount
            var Instructions = new InstructionList();
            var Reader = new MemoryCodeReader(this.Function, 100);
            var Decoder = Iced.Intel.Decoder.Create(32, Reader);
            Decoder.IP = (ulong)Function;
            Instructions.AddRange(Decoder.DecodeMany(JmpSize));


            if (Instructions.IsDangerousToHook()) {
                Log.Warning($"Dangerous Hook {Library}->{Export ?? Ordinal.ToString()} Found; If Crash, try enable the ImportHook");
            }

            var RetAddr = Decoder.IP;



            //Allocate Memory
            var Writer = new MemoryCodeWriter();
            var Compiler = Encoder.Create(32, Writer);

            int HookSize = Instructions.GetEncodedSize(32);

            RealBuffer = new byte[HookSize];
            BypassBuffer = new byte[HookSize + JmpSize];

            DeprotectMemory(Function, (uint)BypassBuffer.LongLength);
            Marshal.Copy(new IntPtr(Function), RealBuffer, 0, RealBuffer.Length);


            //Assemble Bypass
            var phBuffer = BypassBuffer.AllocUnsafe();
            BypassFunction = phBuffer;

            Instructions.Add(Instruction.CreateBranch(Code.Jmp_rel32_32, RetAddr));
            Compiler.Encode(Instructions, (ulong)phBuffer);

            Writer.CopyTo(phBuffer, 0);


            //Assemble the Hook
            Writer = new MemoryCodeWriter();
            Compiler = Encoder.Create(32, Writer);
            var jmp = Instruction.CreateBranch(Code.Jmp_rel32_32, (ulong)HookFunction);
            Compiler.Encode(jmp, (ulong)Function);

            HookBuffer = Writer.ToArray();
        }

#endif

        private void SetupImportHook(void* BaseAddress)
        {
            var Imports = ModuleInfo.GetModuleImports((byte*)BaseAddress);

            var Import = (from x in Imports where x.Function == Export && x.Module.ToLower() == Library.ToLower() select x).Single();

            BypassFunction = Function;
            Function = Import.ImportAddress;

#if x64
            HookBuffer = new byte[8];
            BitConverter.GetBytes((ulong)HookFunction).CopyTo(HookBuffer, 0);
            RealBuffer = new byte[8];
            BitConverter.GetBytes((ulong)BypassFunction).CopyTo(RealBuffer, 0);
#else
            HookBuffer = new byte[4];
            BitConverter.GetBytes((uint)HookFunction).CopyTo(HookBuffer, 0);
            RealBuffer = new byte[4];
            BitConverter.GetBytes((uint)BypassFunction).CopyTo(RealBuffer, 0);
#endif
        }

        public void Install()
        {
            Enabled = true;
            DeprotectMemory(Function, (uint)HookBuffer.LongLength);
            Marshal.Copy(HookBuffer, 0, new IntPtr(Function), HookBuffer.Length);
        }

        public void Uninstall()
        {
            if (!Enabled || Persistent)
                return;

            Enabled = false;
            DeprotectMemory(Function, (uint)RealBuffer.LongLength);
            Marshal.Copy(RealBuffer, 0, new IntPtr(Function), RealBuffer.Length);
        }

        public abstract void Initialize();
    }

    public static partial class Extensions
    {
        public static int GetEncodedSize(this InstructionList List, int bitness, ulong IP = 0)
        {
            var Writer = new MemoryCodeWriter();
            var Compiler = Encoder.Create(bitness, Writer);
            foreach (var Instruction in List)
            {
                Compiler.Encode(Instruction, (ulong)Writer.Count + IP);
            }

            return Writer.Count;
        }
        public static int GetEncodedSize(this Instruction Instruction, int bitness, ulong IP = 0)
        {
            var Writer = new MemoryCodeWriter();
            var Compiler = Encoder.Create(bitness, Writer);
            return (int)Compiler.Encode(Instruction, (ulong)Writer.Count + IP);
        }

        public static uint Encode(this Encoder Encoder, InstructionList List, ulong IP)
        {
            uint Len = 0;
            foreach (var Instruction in List)
                Len += Encoder.Encode(Instruction, IP + Len);
            return Len;
        }

        public unsafe static (T Method, long Address) AssembleMethod<T>(this InstructionList List, int bitness) where T : Delegate
        {
            int Size = List.GetEncodedSize(bitness) * 2;

            long pFunc = (long)Marshal.AllocHGlobal(Size).ToPointer();
            DeprotectMemory((void*)pFunc, (uint)Size);

            var CodeBuffer = new MemoryCodeWriter();
            Encoder Encoder = Encoder.Create(bitness, CodeBuffer);
            if (Encoder.Encode(List, (ulong)pFunc) > Size)
                throw new Exception("Assembler Buffer Overflow");

            CodeBuffer.CopyTo((byte*)pFunc, 0);

            T Delegate = (T)Marshal.GetDelegateForFunctionPointer(new IntPtr(pFunc), typeof(T));

            return (Delegate, pFunc);
        }

        public static InstructionList DecodeMany(this Decoder Decoder, uint MinLength)
        {
            var List = new InstructionList();
            var Begin = Decoder.IP;
            while (Decoder.IP < Begin + MinLength)
            {
                List.Add(Decoder.Decode());
            }
            return List;
        }
        public static InstructionList DecodeAmount(this Decoder Decoder, uint Count)
        {
            var List = new InstructionList();
            while (List.Count < Count)
            {
                List.Add(Decoder.Decode());
            }
            return List;
        }
        public unsafe static Instruction PeekDecode(this Decoder RefDecoder)
        {
            var NewMem = new MemoryCodeReader((void*)RefDecoder.IP);
            var NewDecoder = Decoder.Create(RefDecoder.Bitness, NewMem);
            NewDecoder.IP = RefDecoder.IP;
            return NewDecoder.Decode();
        }
        public unsafe static InstructionList AssembleToList(this Assembler Assembler, ulong RIP)
        {
            var CWBuffer = new MemoryCodeWriter();
            Assembler.Assemble(CWBuffer, RIP);
            var Buffer = CWBuffer.ToArray();
            fixed (void* pBuffer = &Buffer[0])
            {
                using (var Reader = new MemoryCodeReader(pBuffer, (uint)Buffer.Length))
                {
                    var AsmDecoder = Decoder.Create(Assembler.Bitness, Reader);
                    return AsmDecoder.DecodeAmount((uint)Assembler.Instructions.Count);
                }
            }
        }

#if x64
        public static int GetAutoEncodedSize(this InstructionList List, int bitness, ulong IP = 0)
        {
            var Writer = new MemoryCodeWriter();
            var Compiler = Encoder.Create(bitness, Writer);
            return (int)Compiler.AutoEncode(List, IP);
        }
        public static int GetAutoEncodedSize(this Instruction Instruction, int bitness, ulong IP = 0)
        {
            var Writer = new MemoryCodeWriter();
            var Compiler = Encoder.Create(bitness, Writer);
            return Compiler.AutoEncode(Instruction, (ulong)Writer.Count + IP).GetEncodedSize(64);
        }
        public static uint AutoEncode(this Encoder Encoder, InstructionList List, ulong IP)
        {
            uint Len = 0;
            InstructionList NewList = new InstructionList();
            foreach (var Instruction in List)
            {
                var NewInstruction = Encoder.AutoEncode(Instruction, IP + Len);
                NewList.AddRange(NewInstruction);
                Len += (uint)NewInstruction.GetEncodedSize(64, IP + Len);
            }

            Len = 0;
            foreach (var Instruction in NewList)
                Len += Encoder.Encode(Instruction, IP + Len);

            return Len;
        }
        public static InstructionList AutoEncode(this Encoder Encoder, Instruction Instruction, ulong IP)
        {
            InstructionList List = new InstructionList();
            if (Encoder.Bitness <= 32)
            {
                List.Add(Instruction);
            }
            else
            {
                if (Instruction.IsJmp() && !Instruction.IsCallFar && Instruction.Op0Kind != OpKind.Register && Instruction.Op0Kind != OpKind.Memory)
                {

                    if (!Instruction.IsANotJmp())
                        Instruction.NegateConditionCode();

                    var Jmp = Instruction.Immediate64.AssemblyJmp();

                    //Get Far Jmp Size + Short Conditional Jmp Size
                    var JmpSize = (uint)Jmp.GetAutoEncodedSize(64, IP);
                    JmpSize += (uint)Instruction.ConditionCode.ToShortJmp(IP + JmpSize, 64).GetEncodedSize(64, IP);

                    List.Add(Instruction.ConditionCode.ToShortJmp(IP + JmpSize, 64));
                    List.AddRange(Jmp);
                }
                else if (Instruction.IsJmp() && !Instruction.IsCallFar && Instruction.Op0Kind == OpKind.Memory)
                {
                    ulong MemAddress = Instruction.IPRelativeMemoryAddress;

                    //Backup RAX
                    List.Add(Instruction.Create(Code.Push_r64, Register.RAX));

                    //Mov RAX, MemAddress
                    List.AddRange(MemAddress.Assembly_Pushq_imm64());
                    List.Add(Instruction.Create(Code.Pop_r64, Register.RAX));

                    //Mov RAX, [RAX]
                    List.Add(Instruction.Create(Code.Mov_r64_rm64, Register.RAX, new MemoryOperand(Register.RAX)));

                    //Exchange Backup with Jmp Address
                    List.Add(Instruction.Create(Code.Xchg_rm64_r64, new MemoryOperand(Register.RSP), Register.RAX));

                    //Execute Jmp
                    List.Add(Instruction.Create(Code.Retnq));
                }
                else
                {
                    switch (Instruction.Code)
                    {
                        case Code.Mov_r64_rm64:
                            if (Instruction.Op1Kind != OpKind.Memory || Instruction.MemoryBase != Register.RIP)
                                goto default;
                            List.Add(Instruction.Create(Code.Mov_r64_imm64, Instruction.Op0Register, Instruction.IPRelativeMemoryAddress));
                            List.Add(Instruction.Create(Code.Mov_r64_rm64, Instruction.Op0Register, new MemoryOperand(Instruction.Op0Register)));
                            break;
                        case Code.Lea_r64_m:
                            if (Instruction.MemoryBase != Register.RIP)
                                goto default;
                            List.Add(Instruction.Create(Code.Mov_r64_imm64, Instruction.Op0Register, Instruction.IPRelativeMemoryAddress));
                            break;
                        default:
                            List.Add(Instruction);
                            break;
                    }
                }
            }
            return List;
        }

        public static bool IsCondJmp(this Instruction Instruction) => Instruction.ConditionCode switch
        {
            ConditionCode.None => false,
            _ => true
        };
        public static bool IsANotJmp(this Instruction Instruction) => Instruction.ConditionCode switch
        {
            ConditionCode.ne => true,
            ConditionCode.no => true,
            ConditionCode.np => true,
            ConditionCode.ns => true,
            _ => false
        };
        public static Instruction ToShortJmp(this ConditionCode Condition, ulong Address, byte bitness)
        {
            bool x64 = bitness == 64;
            return (Condition, bitness) switch
            {
                //x64
                (ConditionCode.a, 64) => Instruction.CreateBranch(Code.Ja_rel32_64, Address),
                (ConditionCode.ae, 64) => Instruction.CreateBranch(Code.Jae_rel32_64, Address),
                (ConditionCode.b, 64) => Instruction.CreateBranch(Code.Jb_rel32_64, Address),
                (ConditionCode.be, 64) => Instruction.CreateBranch(Code.Jbe_rel32_64, Address),
                (ConditionCode.e, 64) => Instruction.CreateBranch(Code.Je_rel32_64, Address),
                (ConditionCode.g, 64) => Instruction.CreateBranch(Code.Jg_rel32_64, Address),
                (ConditionCode.ge, 64) => Instruction.CreateBranch(Code.Jge_rel32_64, Address),
                (ConditionCode.l, 64) => Instruction.CreateBranch(Code.Jl_rel32_64, Address),
                (ConditionCode.ne, 64) => Instruction.CreateBranch(Code.Jne_rel32_64, Address),
                (ConditionCode.no, 64) => Instruction.CreateBranch(Code.Jno_rel32_64, Address),
                (ConditionCode.np, 64) => Instruction.CreateBranch(Code.Jnp_rel32_64, Address),
                (ConditionCode.ns, 64) => Instruction.CreateBranch(Code.Jns_rel32_64, Address),
                (ConditionCode.o, 64) => Instruction.CreateBranch(Code.Jo_rel32_64, Address),
                (ConditionCode.p, 64) => Instruction.CreateBranch(Code.Jp_rel32_64, Address),
                (ConditionCode.s, 64) => Instruction.CreateBranch(Code.Js_rel32_64, Address),

                //x32                
                (ConditionCode.a, 32) => Instruction.CreateBranch(Code.Ja_rel32_32, Address),
                (ConditionCode.ae, 32) => Instruction.CreateBranch(Code.Jae_rel32_32, Address),
                (ConditionCode.b, 32) => Instruction.CreateBranch(Code.Jb_rel32_32, Address),
                (ConditionCode.be, 32) => Instruction.CreateBranch(Code.Jbe_rel32_32, Address),
                (ConditionCode.e, 32) => Instruction.CreateBranch(Code.Je_rel32_32, Address),
                (ConditionCode.g, 32) => Instruction.CreateBranch(Code.Jg_rel32_32, Address),
                (ConditionCode.ge, 32) => Instruction.CreateBranch(Code.Jge_rel32_32, Address),
                (ConditionCode.l, 32) => Instruction.CreateBranch(Code.Jl_rel32_32, Address),
                (ConditionCode.ne, 32) => Instruction.CreateBranch(Code.Jne_rel32_32, Address),
                (ConditionCode.no, 32) => Instruction.CreateBranch(Code.Jno_rel32_32, Address),
                (ConditionCode.np, 32) => Instruction.CreateBranch(Code.Jnp_rel32_32, Address),
                (ConditionCode.ns, 32) => Instruction.CreateBranch(Code.Jns_rel32_32, Address),
                (ConditionCode.o, 32) => Instruction.CreateBranch(Code.Jo_rel32_32, Address),
                (ConditionCode.p, 32) => Instruction.CreateBranch(Code.Jp_rel32_32, Address),
                (ConditionCode.s, 32) => Instruction.CreateBranch(Code.Js_rel32_32, Address),

                //x16
                (ConditionCode.a, 16) => Instruction.CreateBranch(Code.Jae_rel16, Address),
                (ConditionCode.ae, 16) => Instruction.CreateBranch(Code.Jae_rel16, Address),
                (ConditionCode.b, 16) => Instruction.CreateBranch(Code.Jb_rel16, Address),
                (ConditionCode.be, 16) => Instruction.CreateBranch(Code.Jbe_rel16, Address),
                (ConditionCode.e, 16) => Instruction.CreateBranch(Code.Je_rel16, Address),
                (ConditionCode.g, 16) => Instruction.CreateBranch(Code.Jg_rel16, Address),
                (ConditionCode.ge, 16) => Instruction.CreateBranch(Code.Jge_rel16, Address),
                (ConditionCode.l, 16) => Instruction.CreateBranch(Code.Jl_rel16, Address),
                (ConditionCode.ne, 16) => Instruction.CreateBranch(Code.Jne_rel16, Address),
                (ConditionCode.no, 16) => Instruction.CreateBranch(Code.Jno_rel16, Address),
                (ConditionCode.np, 16) => Instruction.CreateBranch(Code.Jnp_rel16, Address),
                (ConditionCode.ns, 16) => Instruction.CreateBranch(Code.Jns_rel16, Address),
                (ConditionCode.o, 16) => Instruction.CreateBranch(Code.Jo_rel16, Address),
                (ConditionCode.p, 16) => Instruction.CreateBranch(Code.Jp_rel16, Address),
                (ConditionCode.s, 16) => Instruction.CreateBranch(Code.Js_rel16, Address),

                //Uncoditionall
                (ConditionCode.None, 64) => Instruction.CreateBranch(Code.Jmp_rel32_64, Address),
                (ConditionCode.None, 32) => Instruction.CreateBranch(Code.Jmp_rel32_32, Address),
                (ConditionCode.None, 16) => Instruction.CreateBranch(Code.Jmp_rel16, Address),

                _ => throw new InvalidOperationException()
            };
        }

        public static bool IsJmp(this Instruction Instruction) =>
            Instruction.IsJmpNear || Instruction.IsJmpNearIndirect ||
            Instruction.IsJmpShort || Instruction.IsJmpFar ||
            Instruction.IsJmpFarIndirect || Instruction.IsJmpShortOrNear ||
            Instruction.IsJccNear || Instruction.IsJccShort ||
            Instruction.IsJccShortOrNear;

        public static bool IsRet(this Instruction Instruction) => Instruction.Code switch
        {
            Code.Retfd => true,
            Code.Retfd_imm16 => true,
            Code.Retfq => true,
            Code.Retfq_imm16 => true,
            Code.Retfw => true,
            Code.Retfw_imm16 => true,
            Code.Retnd => true,
            Code.Retnd_imm16 => true,
            Code.Retnq => true,
            Code.Retnq_imm16 => true,
            Code.Retnw => true,
            Code.Retnw_imm16 => true,
            _ => false
        };

        public static InstructionList Assembly_Pushq_imm64(this ulong Value)
        {
            var Instructions = new InstructionList();
            Instructions.Add(Instruction.Create(Code.Pushq_imm32, unchecked((int)(Value & uint.MaxValue))));
            Instructions.Add(Instruction.Create(Code.Mov_rm32_imm32, new MemoryOperand(Register.RSP, 4L), (uint)(Value >> 8 * 4)));
            return Instructions;
        }

        public static InstructionList AssemblyJmp(this ulong Target)
        {
            var Instructions = new InstructionList();
            Instructions.AddRange(Target.Assembly_Pushq_imm64());
            Instructions.Add(Instruction.Create(Code.Retnq));
            return Instructions;
        }
#endif

        public static bool IsDangerousToHook(this InstructionList List)
        {
            int Dummies = 0;
            var DummyList = new Code[] { Code.Int3, Code.Nopd, Code.Nopq, Code.Nopw };

            foreach (var Instruction in List)
            {
                if (DummyList.Contains(Instruction.Code))
                {
                    Dummies++;
                    continue;
                }

                if (Dummies != 0)
                    return true;
            }
            return false;
        }

        public unsafe static byte* AllocUnsafe(uint Bytes)
        {
            return VirtualAlloc(null, Bytes, AllocationType.Reserve | AllocationType.Commit, MemoryProtection.ExecuteReadWrite); ;
        }
        public unsafe static byte* AllocUnsafe(this IEnumerable<byte> Buffer)
        {
            var tmp = Buffer.ToArray();
            var Addr = VirtualAlloc(null, (uint)tmp.Length, AllocationType.Reserve | AllocationType.Commit, MemoryProtection.ExecuteReadWrite);
            for (int i = 0; i < tmp.Length; i++)
                *(Addr + i) = tmp[i];
            return Addr;
        }

        public unsafe static void DeprotectMemory(void* Buffer, uint Length, bool ExecutableOnly = false)
        {
            VirtualProtect(Buffer, Length, ExecutableOnly ? MemoryProtection.ExecuteRead : MemoryProtection.ExecuteReadWrite, out _);
        }
        public unsafe static void* GetLibrary(string Library)
        {
            var hModule = GetModuleHandle(Library);
            if (hModule != null)
                return hModule;
            return LoadLibrary(Library);
        }

        public unsafe static void* LoadLibrary(string Library) => LoadLibraryW(Library);
        public unsafe static void* GetModuleHandle(string Library) => GetModuleHandleW(Library);
        public unsafe static void* GetProcAddress(void* hModule, string ProcName) => GetProcAddressExt(hModule, ProcName);
        public unsafe static void* GetProcAddress(void* hModule, ushort ProcOrd) => GetProcAddressExt(hModule, ProcOrd);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        unsafe static extern void* LoadLibraryW(string Library);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        unsafe static extern void* GetModuleHandleW(string Library);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        unsafe static extern void* GetProcAddressExt(void* hModule, string ProcName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        unsafe static extern void* GetProcAddressExt(void* hModule, ushort ProcOrdinal);

        [DllImport("kernel32.dll")]
        unsafe static extern bool VirtualProtect(void* lpAddress, uint dwSize, MemoryProtection flNewProtect, out AllocationType lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        unsafe static extern byte* VirtualAlloc(void* lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll")]
        public unsafe static extern bool IsBadCodePtr(void* Ptr);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }
    }

    public unsafe static class ModuleInfo
    {
        public unsafe static CodeInfo GetCodeInfo(byte* hModule)
        {
            ulong PEStart = *(uint*)(hModule + 0x3C) + (ulong)hModule;
            ulong OptionalHeader = PEStart + 0x18;

            uint SizeOfCode = *(uint*)(OptionalHeader + 0x04);
            uint EntryPoint = *(uint*)(OptionalHeader + 0x10);
            uint BaseOfCode = *(uint*)(OptionalHeader + 0x14);

            return new CodeInfo()
            {
                CodeAddress = hModule + BaseOfCode,
                EntryPoint = hModule + EntryPoint,
                CodeSize = SizeOfCode
            };
        }

        public unsafe static ImportEntry[] GetModuleImports(byte* Module)
        {
            if (Module == null)
                throw new Exception("Invalid Module...");

            uint PtrSize = Environment.Is64BitProcess ? 8u : 4u;

            ulong OrdinalFlag = (1ul << (int)((8 * PtrSize) - 1));

            ulong PEStart = *(uint*)(Module + 0x3C);
            ulong OptionalHeader = PEStart + 0x18;

            ulong ImageDataDirectoryPtr = OptionalHeader + (PtrSize == 8 ? 0x70u : 0x60u);

            ulong ImportTableEntry = ImageDataDirectoryPtr + 0x8;

            uint RVA = (uint)ImportTableEntry;

            uint* ImportDesc = (uint*)(Module + *(uint*)(Module + RVA));

            if (ImportDesc == Module)
                return new ImportEntry[0];

            List<ImportEntry> Entries = new List<ImportEntry>();

            while (true)
            {
                uint OriginalFirstThunk = ImportDesc[0];
                uint Name = ImportDesc[3];
                uint FirstThunk = ImportDesc[4];

                if (OriginalFirstThunk == 0x00)
                    break;

                string ModuleName = Marshal.PtrToStringAnsi(new IntPtr(Module + Name));

                void** DataAddr = (void**)(Module + OriginalFirstThunk);
                void** IATAddr = (void**)(Module + FirstThunk);
                while (true)
                {
                    void* EntryPtr = *DataAddr;

                    if (EntryPtr == null)
                        break;

                    bool ImportByOrdinal = false;
                    if (((ulong)EntryPtr & OrdinalFlag) == OrdinalFlag)
                    {
                        EntryPtr = (void*)((ulong)EntryPtr ^ OrdinalFlag);
                        ImportByOrdinal = true;
                    }
                    else
                        EntryPtr = (void*)(Module + (ulong)EntryPtr);

                    ushort Hint = ImportByOrdinal ? (ushort)EntryPtr : *(ushort*)EntryPtr;

                    string ExportName = null;
                    if (!ImportByOrdinal)
                        ExportName = Marshal.PtrToStringAnsi(new IntPtr(unchecked((long)EntryPtr + 2)));

                    Entries.Add(new ImportEntry()
                    {
                        Function = ExportName,
                        Ordinal = Hint,
                        Module = ModuleName,
                        ImportAddress = IATAddr,
                        FunctionAddress = *IATAddr
                    });

                    DataAddr++;
                    IATAddr++;
                }


                ImportDesc += 5;//sizeof(_IMAGE_IMPORT_DESCRIPTOR)
            }

            return Entries.ToArray();
        }
    }
    public unsafe struct CodeInfo
    {
        /// <summary>
        /// The Begin Address of the module code
        /// </summary>
        public void* CodeAddress;
        /// <summary>
        /// The Size of the module code
        /// </summary>
        public uint CodeSize;
        /// <summary>
        /// The Entry Point of the module
        /// </summary>
        public void* EntryPoint;

        public void* EndCodeAddress => (void*)((ulong)CodeAddress + CodeSize);
        public bool AddressIsContained(void* Address) => Address >= CodeAddress && Address <= EndCodeAddress;
    }

    public unsafe struct ImportEntry
    {

        /// <summary>
        /// The Imported Module Name
        /// </summary>
        public string Module;

        /// <summary>
        /// The Imported Function Name
        /// </summary>
        public string Function;

        /// <summary>
        /// The Import Ordinal Hint
        /// </summary>
        public ushort Ordinal;

        /// <summary>
        /// The Address of this Import in the IAT (Import Address Table)
        /// </summary>
        public void* ImportAddress;

        /// <summary>
        /// The Address of the Imported Function
        /// </summary>
        public void* FunctionAddress;
    }
}