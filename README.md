# Tzix
**Tzix** is a launcher, which launches the file you want to launch by typing a part of the name. (**Tzix** は、ファイルの名前の一部を入力するだけでファイルを起動できるランチャー。)

## Usage
### Execute
#### Windows
Execute ``tzix.exe``. (``tzix.exe`` を実行する。)

#### Mac/Linux
Install Mono and run ``mono tzix.exe`` command. (Mono をインストールして、 ``mono tzix.exe`` コマンドを実行する。)

### Database generation
At the first time execution, **Tzix** generates a database for quick searching. This takes slightly long time. (最初の起動時には、高速に検索するためのデータベースが生成される。これには少し長い時間がかかる。)

### Search
**Tzix** shows an input box. As you type a character **Tzix** searches files whose name contains the word you've typed. (入力欄が表示される。文字を入力するごとに、入力した語を名前に含むファイルが検索される。)

### Execute
Type the Enter/Return key to execute the selected file/directory. Or it just vanishes from the list if the execution failed. (Enter/Returnキーを入力すると、選択されているファイルやディレクトリが開かれる。実行に失敗すると、それは単に一覧から消える。)

**Tzix** prioritizes listing files you've executed in the past. (以前に実行したことがあるファイルは優先して表示される。)

### Browse a directory
In "directory browsing mode", **Tzix** searches for files directly inside the directory. And it updates the data related to the directory in the database. (ディレクトリ閲覧モードでは、そのディレクトリの直下にあるファイルだけが検索される。また、データベース内のこのディレクトリに関するデータが更新される。)

To browse a directory, select it and type ``Ctrl+→``; or double click it. ``Ctrl+←`` to browse the parent directory of the selected file. (ディレクトリを閲覧するには、それを選択した状態で ``Ctrl+→`` を入力するか、それをダブルクリックすればよい。 ``Ctrl+←`` を入力すると、選択されたファイルやディレクトリの親ディレクトリを閲覧できる。)

## License
[MIT license](LICENSE.md)
