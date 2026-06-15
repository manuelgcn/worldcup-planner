# Dependencias externas

## Exportación CSV/XLSX

| Paquete | Versión | Licencia | Uso |
|---|---:|---|---|
| [CsvHelper](https://www.nuget.org/packages/CsvHelper/33.1.0) | 33.1.0 | MS-PL o Apache-2.0 | Generación de CSV con encabezados, cultura invariante y escape correcto de campos. |
| [ClosedXML](https://www.nuget.org/packages/ClosedXML/0.105.0) | 0.105.0 | MIT | Generación de archivos XLSX sin requerir Microsoft Excel instalado. |

Se eligió ClosedXML frente a EPPlus porque su licencia MIT no requiere configuración comercial y su API permite generar libros XLSX tipados de forma directa.
