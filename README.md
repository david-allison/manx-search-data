# manx-search-data

A collection of computer-readable bilingual texts and audio transcriptions for the Manx Language. 

The aim of this repository is to contain the entire corpus of the Manx Language, paying specific attention to classical Manx texts and audio from native speakers, to guide further revival of the language.

Hosted by [manx-corpus-search](https://github.com/david-allison-1/manx-corpus-search)

## Goals

* Allow easy access to the written works of classical Manx speakers
  * A reference of native use of text for professionals
  * A curated collection of bilingual texts for learners
  * Structured data for academic/technological use-cases
* Highlight works by classical Manx speakers (pre-1908), to avoid unintentional inclusion of non-native constructs while learning and teaching Manx.
* Foster and direct collaboration 
  * Allow anyone to suggest edits to translations (via GitHub)

### Long-term goals

* Contain all pre-1908 Manx literature 
* Contain all transcriptions of classical Manx speakers
* Contain all modern works of Manx literature and transcriptions of native recordings where copyright/permission has been obtained.

## Contributing

See https://github.com/david-allison-1/manx-search-data/blob/master/CONTRIBUTING.md

In summary, open a [discussion](https://github.com/david-allison-1/manx-search-data/discussions), introduce yourself and we'll help you get started. Your contributions are much appreciated!

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
