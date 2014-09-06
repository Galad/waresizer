waresizer
=========

A simple command line tool allowing resizing Windows Store apps a assets in order to support all scales.
It can resize well known manifest assets, as well as custom assets.

There is not any check of the image ratio so you need to make sure that the source image has the correct ratio.

#Usage examples

Creating assets for all the square tiles. 
`waresizer "C:\MyAppAssets\Logo.png" /dest "C:\MyAppAssets\" /sizes Wa81Square70x70,Wa81Square150x150,Wa81Square310x310,Wa81Square30x30,Wa81Store`

Creating assets for a custom size. The target at scale 100% is 200x250.
`waresizer "C:\MyAppAssets\Asset.png" /dest "C:\MyAppAssets\" /w 200 /h 250`
It creates the scales for all platforms.

Creating assets for a custom size, only for Windows 8.1.
`waresizer "C:\MyAppAssets\Asset.png" /dest "C:\MyAppAssets\" /w 200 /h 250 /wa81`

Creating assets for a custom size, only for Windows Phone 8.1.
`waresizer "C:\MyAppAssets\Asset.png" /dest "C:\MyAppAssets\" /w 200 /h 250 /wpa81`

#Sizes

##Windows 8.1

- Wa81Square70x70	
- Wa81Square150x150	
- Wa81Wide310x150	
- Wa81Square310x310	
- Wa81Square30x30	
- Wa81Store			
- Wa81SplashScreen	
- Wa81Badge	

##Windows Phone 8.1

- Wpa81Square71x71	
- Wpa81Square150x150
- Wpa81Wide310x150	
- Wpa81Square44x44	
- Wpa81Store		
- Wpa81SplashScreen	
- Wpa81Badge		
