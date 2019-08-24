# CSHTML Tokenizer

CSHTMLTokenizer tokenizes CSHTML into tokens. For now (V0.1.4) only HTML is tokenized. Tokenization is done 
based on the [HTML Tokenization spec](https://html.spec.whatwg.org/multipage/parsing.html#tokenization). Only those
parts that are relevant to CSHTML tokenization is supported:

## Html

* Data
* Tags
* Attributes

## CSHtml

* Variables in attributes @
* Statements in attributes @(...)

## CSS

* Classes
* Declarations

## Usage

```
 var tokens = Tokenizer.Parse("This is a test!<br>This is a <b n='v'>bold</b>");
```
