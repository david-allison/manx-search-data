# manx-search-data

A collection of structured bilingual texts for the Manx Language. 

The aim of this repository is to contain the entire corpus of the Manx Language, paying specific attention to texts written by native speakers, to guide the revival of the language.

## Goals

* Allow easy access to the written works of Native Manx speakers
  * A reference of native use of text for professionals
  * A curated collection of bilingual texts for learners
  * Structured data for academic/technological use-cases
* Highlight works by native speakers (pre-1908), to avoid unintentional inclusion of non-native constructs while learning and teaching Manx.
* Foster and direct collaboration 
  * Allow anyone to suggest edits to translations (via GitHub)

## Contributing

### One-time setup

* Create a GitHub account
* Select "Fork" on the top right hand corner of this page
* Download and run "GitHub Desktop": https://desktop.github.com/
* Log in to GitHub
* Clone Existing Repository
* Select `manx-search-data`

### Adding a new document

* Select "manx-search-data" Repository inside GitHub Desktop
* Select "master" branch
  * If there are pending changes (typically due to Excel keeping a file open: `Close Excel` and in GtiHub Desktop Right `Click the change - Discard the changes`)
* Select "Fetch Origin" (top of the screen)
* Select "Pull" if the option appears
* Branch - New Branch
   * name: doesn't matter much. Aim to avoid non-English letters and punctuation. Sample: `60-Manx-Carvals-Lhig-ainelyn-heose-as-nooghyn-wass`
* In the top menu: `Repository - Show In Explorer`
* Open the "OpenData" folder, and navigate to the template: `OpenData/NedBeg/`. Select a folder at random containing the 3 required files: `manifest.json.txt`, `license.txt`, `document.csv`
* Make a new folder somewhere under `OpenData`, and copy the 3 files
* Generate a new `document.csv` acording to the documentation (((TODO)**
* Edit `license.txt` and state the licenses of the work. 
   * If you created the translation, feel free to use the existing license in the template (Open Govt/Creative Commons license) and change the name/year. 
   * If you did not create the work, ensure you have the author's permission to apply an appropriate license/copyright
* Edit `manifest.json.txt` to define the name/date/identifier of the document
   * The first 3 fields are mandatory. 
     * `"created"` can be replaced with `createdCircaStart` and `createdCircaEnd` if the date is unknown
     * `ident` must be unique throughout the corpus
     * `name` is displayed as the name of the Work in the corpus
   * Any other useful infomration can be included afterwards with an arbitrary quoted identifier
* Open GitHub Desktop and check your changes
   * If you copied the Excel sheet, ensure that you correctly removed all of the rows from the source
   * Ensure the license is correct
   * Ensure `manifest.json.txt` contains correct information
* Setup a commit
  * Enter a summary, typically: `Add: "Document Name"`
  * Enter a description
    * If there is an associated issue, enter `Fixes #IssueNumber`
    * Otherwise, leave it blank
* Click `Commit to <branch>`
* Click "Publish"
* Click "Create Pull Request"
* Click "Create Pull Request" in your Browser
* Ensure all checks pass. 
  * If they don't, select "Details". 
* Wait for a maintainer to approve and merge the Pull Request

If you need any help, open an issue above, and write `@david-allison-1` in the issue body. See the files in `OpenData` for existing works.

## Technical Considerations

* All code is related to CI, checking that uploads are correct
* All Data is under the OpenData folder
* Ensure that works can be added by:
   * Non-technical users
   * With no admin access on their PC
* The corpus is stored as one "work" per-folder: 
    * csv for bi/trilingual translation (additional columns for human-editable data such as page number, or notes per-line)
    * metadata in .json.txt (designed to open in the system text editor without setting file associations)
    * future data (POS tags etc..) will either be stored as separate files in the folder if computer-readable, or alongside the data if human editable