## AnkaPTT
**安価(アンカ) tool for PTT**

AnkaPTT is a tool that help people playing 安価 on PTT. It parses PTT post to fetch pushes(responses) on website And it can be set anka rules to filter pushes. 

AnkaPTT is built in .NET framework 4.5.2 in C#.

AnkaPTT is using CefSharp (https://github.com/cefsharp/CefSharp) for webbrowser.

## How to use

* 於最上方的網址列輸入ptt web 網址，並按下Enter

* 左側會出現網頁，會自動啟動推文自動更新

* 右方清單會出現解析推文、並套用下方篩選器後的結果

* 右側清單用滑鼠左鍵連點，網頁會快轉至該推文位置，並改變底色強調

* 篩選器是從最上面套用到最下面，目前無法調整順序

* 篩選器的輸入框，可以使用鍵盤↑↓來調整數值

* 如果網頁版的自動更新推文功能故障，可使用[自動重新載入]功能，預設為10秒刷新一次

* 可開最多5個額外的篩選視窗，且分別對網頁推文改變不同的底色強調

## 注意事項

* 基於網頁版實作，如果因為修文關係，破壞了推文格式，解析會失敗

* 偶爾網頁版的自動更新推文功能會故障，可使用[自動重新載入]功能

* [自動重新載入]功能載入網頁時，會檢查頁面是否有更新，若無更新則不重新整理畫面

## Todo list:

* 全域設定檔

* 篩選器設定檔

* 分離一些混在 ViewModel 裡面的業務邏輯程式碼 (不過我有點懶)

* 調整[自動重新載入]功能與更新推文方式的模式

* MainViewModel - FilterViewModel - FilterView 這邊的關係寫法很醜，希望能重構
