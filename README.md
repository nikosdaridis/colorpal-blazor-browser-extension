<div align="center">
  <a href="https://chromewebstore.google.com/detail/colorpal-color-picker-eye/mbnpegpimodgjmlbfhkkdgbcfjmgpoad"><img alt="ColorPal logo" src="images/Logo.png" width="100"></a>
  <h1>ColorPal</h1>
  <p>Browser extension to pick colors from the screen, make tints and shades and download your palette!</p>
  <p>Built with <a href="https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor">Blazor</a>, <a href="https://www.typescriptlang.org">TypeScript</a> and <a href="https://tailwindcss.com">Tailwind CSS</a>. Up to <a href="https://github.com/nikosdaridis/colorpal-blazor-browser-extension/tree/1.3.7">v1.3.7</a> it was built with TypeScript, HTML and CSS.</p>
  <a href="https://chromewebstore.google.com/detail/colorpal-color-picker-eye/mbnpegpimodgjmlbfhkkdgbcfjmgpoad">
    <picture>
      <source srcset="https://i.imgur.com/XBIE9pk.png" media="(prefers-color-scheme: dark)">
      <img height="70" src="https://i.imgur.com/oGxig2F.png" alt="Available in the Chrome Web Store">
    </picture>
  </a>
  <p>
    <img alt="Saved colors" height="280" src="images/Screenshot1.jpg">
    <img alt="Color picker" height="280" src="images/Screenshot2.jpg">
    <img alt="Tints and shades" height="280" src="images/Screenshot3.jpg">
    <img alt="Settings" height="280" src="images/Screenshot4.jpg">
  </p>
</div>

## ✨ Features

- Pick any color on your screen with the eyedropper, or choose one with the color picker
- Copy its code as HEX, RGB, HSL, HSV or a CSS filter
- See its name, from over 25,000 color names
- Save colors to a palette, rearrange them and download the palette as PNG or CSV
- Get tints and shades of any color
- Light and Dark themes

## 🛠 Development

Requires the [.NET 11 SDK](https://dotnet.microsoft.com/download/dotnet/11.0) and [Node.js 24](https://nodejs.org/en/download). Run every command from the repository root.

1. Start the app as a web page. The first run also installs the npm packages:

2. To recompile TypeScript and Tailwind CSS as you edit them, run this in a second terminal and reload the page after each change:

   ```sh
   npm run dev-watch --prefix ColorPal
   ```

## 🚀 Build For Production (Windows)

1. Publish the extension:

   ```sh
   dotnet publish ColorPal/ColorPal.csproj -c Release
   ```

   This creates `colorpal_<version>.zip` and a `colorpal_<version>` folder in `ColorPal/bin/Release/net11.0/publish`. For a smaller zip, first run `dotnet workload install wasm-tools`.

2. To try it, open `chrome://extensions`, turn on Developer mode, choose Load unpacked and select the folder. Its full path must stay under 180 characters, or the popup won't start.

## 🎨 Color Names

The names live in `ColorPal/wwwroot/Data/colorNamesStep4.dat`. To rebuild that file from the latest [color-name-list](https://github.com/meodai/color-names), run:

```sh
dotnet run --project Helpers/ColorNamesBinaryMap/ColorNamesBinaryMap.csproj -- --step 4 --source https://unpkg.com/color-name-list/dist/colornames.json --out ColorPal/wwwroot/Data
```

`--step` is the RGB precision from 1 to 255, and the extension loads step 4. Leave out any argument and the helper asks for it.

## 📄 License

[MIT](LICENSE.md)
