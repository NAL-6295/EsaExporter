# EsaExporter
- esaの記事を一旦画像ごとローカルに保存するツール

実行するには以下
```
dotnet EsaExporter.dll token teamname option
```

optionは
- Json
    - Json形式で一旦Postとimageを保存するので後でどうとでもできるやつ
- Markdown 
    - md形式で保存するが、他の情報は落ちる。
    - ダウンロードしたイメージに対するパスも解決する
    - esaの記事間のリンクもローカルで解決する
    - 全記事の索引用にindex.mdを生成するｓ