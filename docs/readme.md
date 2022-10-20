# Omtv - One More Table Visualizer

![Build](https://github.com/tihilv/Omtv/actions/workflows/dotnet.yml/badge.svg) 
![Nuget](https://img.shields.io/nuget/v/Omtv.Api)

Omtv is a .NET-running text-to-table visualizer engine. It transforms structured HTML-like text to an output that can be rendered via plugins to HTML, PDF, Excel...

Omtv uses html-based approach for document markup, but the function list is significantly limited to make the implementation for different formats easier.

## Reference Guide

To perform the transformation the following code can be used:
```C#
ITableOutput output = new HtmlTableOutput(outputStream);
await TableVisualizer.TransformAsync(inputStream, output);
```

At the current moment the following outputs are implemented:
- `CsvTableOutput`,
- `HtmlTableOutput`.

Under development are the outputs for:
- PDF,
- MS Excel.

The processing is being performed by a single pass, so implementations of `ITableOutput` can use memory-efficient streaming. 

### Document Template
A document has a header that expresses default document parameters and list of commonly used styles. Then a list of tables is expected. A table contains a list of rows, every row consists of cells:
```
<document>
    <header width="297mm" height="210mm" name="Some name">
        <style name="default" backColor="white" foreColor="black"/>
        <style name="odd" backColor="gray"/>
        <style name="borders" border="2px"/>
    </header>

    <table name="Table 1" border="5px" border.right="7px blue">
        <row styles="odd" height="25px"><cell align="center" width="70%" styles="borders">v11</cell><cell rowSpan="2" valign="after" width="20px">v12</cell><cell>v13</cell></row>
        <row><cell align="center">v21</cell><cell>v23</cell></row>
    </table>
</document>
```

### Header (\<header\>)
Header section contains the following properties:

| Name       | Type      | Comment                                                                                                                  |
|------------|-----------|--------------------------------------------------------------------------------------------------------------------------|
| `name`     | 'String'  | Name of the document. Can be used, for example, as a name of the HTML document.                                          |
| `width`    | 'Measure' | Width of the document page.                                                                                              |
| `height`   | 'Measure' | Height of the document page.                                                                                             |
| `margin`   | 'Measure' | Margin of the document page. It's possible to set/override borders partially:<br/>'margin.[left / right / top / bottom]' |
| -Children- | 'Style[]' | Set of predefined styles of the document.                                                                                |

### Style (\<style\>)
A style can be expressed in two forms: as a separate element of the header, or as a property set of a table, row or cell (embedded style).

| Name        | Type        | Comment                                                                                                                       |
|-------------|-------------|-------------------------------------------------------------------------------------------------------------------------------|
| `name`      | 'String'    | Name of the style. Can be used only for a style of the document header.                                                       |
| `styles`    | 'String'    | Semicolumn separated list of the parent style names. Can be used for embedded styles only.                                    |
| `backColor` | 'Color'     | Background color.                                                                                                             |
| `foreColor` | 'Color'     | Foreground color.                                                                                                             |
| `font`      | 'Font'      | Font.                                                                                                                         |
| `algin`     | 'Alignment' | Horizontal alignment.                                                                                                         |
| `valgin`    | 'Alignment' | Vertical alignment.                                                                                                           |
| `border`    | 'Border'    | Borders around the element. It's possible to set/override borders partially:<br/>'border.[left / right / top / bottom]' |

### Table (\<table\>)
Every table is supposed to be stretched to the document width.

Table section contains the following properties:

| Name       | Type     | Comment                                                                    |
|------------|----------|----------------------------------------------------------------------------|
| `name`     | 'String' | Name of the table. Can be used, for example, as a name of the Excel sheet. |
| -Children- | 'Row[]'  | Set of rows of the table.                                                  |

### Row (\<row\>)
Row section contains the following properties:

| Name       | Type     | Comment                                           |
|------------|----------|---------------------------------------------------|
| `height`   | 'Measure' | Height of the row.                               |
| `header`   | 'any'    | Is the row a header. Can be used for HTML format. |
| -Children- | 'Cell[]' | Set of cells of the row.                          |

### Cell (\<cell\>)
Row section contains the following properties:

| Name      | Type      | Comment                                               |
|-----------|-----------|-------------------------------------------------------|
| `width`   | 'Measure' | Width of the cell.                                    |
| `rowSpan` | 'Integer' | Number of rows to merge including the current one.    |
| `colSpan` | 'Integer' | Number of columns to merge including the current one. |
| `header`   | 'any'    | Is the cell a header. Can be used for HTML format.    |
| -Content- | 'String'  | Value of the cell.                                    |

Merged by rowSpan and colSpan cells should not present in the document. 

If row is a header, a cell becomes a header automatically.

### Primitives

#### Alignment
Represents horizontal and vertical text alignment. The possible values are:
- before (left or top),
- center,
- after (right or bottom).

#### Border
Border is expressed by line thickness and color:
- 3px (border of 3px in  default color),
- 2px red (border 2px thick in red),
- \- blue (border of inherited thickness in blue).

#### Color
Color can be expressed in two ways: 
- Hex form (##FFFFFF),
- Color name (white). See [color mapping](https://learn.microsoft.com/en-us/dotnet/api/system.drawing.color).

#### Font
Font is expressed by family name and size:
- 'Times new roman' 14

#### Measure
Measure represents a value with unit. Supported units are:
- Percents (50%),
- Pixels (15 px),
- Em (3 em),
- Millimeters (297 mm).
