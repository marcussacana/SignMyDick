# Real Sigin Crack
My successful attempt to crack the signature hang, unlike the main program that just hides the alert, so you can load any unsigned CRX3 as long as it has an "update_url" in the manifest.

### Why not included as feature in the main program?
There are some points that i need to take attention before include this as feature,
1 - How much the algorithm change after a new update;
- I lost 2 days analyzing the edge to found where I can patch
- If I do a auto-patcher by pattern searching, the smallest of changes of in the source code can cause a huge butterfly effect on the optimizer, changing all registers, memory addresses and more, this mean my patch don't will work while I don't update again  

2 - As expected, my pattern match from the edge don't worked with the chrome, I will really need research every browser to every update to patch it?


### So, it's better forget a real patching?
Well, not really, just isn't very probabbly, I want write a tool that can generate the patch automatically to every single update of the browser, isn't imposible, but isn't easy.

### Note for researchers:
- Here is where the browser try enable the extension, and is here where he check with the **MustRemainDisabled** if this extension can be enabled
![https://github.com/marcussacana/SignMyDick/blob/master/Real%20Sigin%20Crack/EnableExtension.png?raw=true](https://github.com/marcussacana/SignMyDick/blob/master/Real%20Sigin%20Crack/EnableExtension.png?raw=true)
- Here is the **MustRemainDisabled**, where the browser determine if the user can enable or not this extension
![https://github.com/marcussacana/SignMyDick/blob/master/Real%20Sigin%20Crack/MustRemainDisabled.png?raw=true](https://github.com/marcussacana/SignMyDick/blob/master/Real%20Sigin%20Crack/MustRemainDisabled.png?raw=true)
 - After the extension be verified, here is where he call the **DisableExtension** if the signature check fails
 ![https://github.com/marcussacana/SignMyDick/blob/master/Real%20Sigin%20Crack/DisableExtension.png?raw=true](https://github.com/marcussacana/SignMyDick/blob/master/Real%20Sigin%20Crack/DisableExtension.png?raw=true)
 So, we need 2 patches, where they are:
 1 - Prevent the **DisableExtension** to be called  
 (Very easy, I just put a `ret` and he don't disable the extension anymore)
 2 - Ensure the **MustRemainDisabled** return false
 (In this my build instead of RAX he used RBX to return the boolean, I think this is a out parameter, Or in the wrost case this is a change by the optimizator?, anyway here I did `xor rbx, rbx` and then `ret`; and everything worked like a charm)
![https://github.com/marcussacana/SignMyDick/blob/master/Real%20Sigin%20Crack/Patches.png?raw=true](https://github.com/marcussacana/SignMyDick/blob/master/Real%20Sigin%20Crack/Patches.png?raw=true)

So, it's this for now, I hope this help you :)
