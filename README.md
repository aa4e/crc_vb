# crc_vb

This project realizes algorithm of CRC count by **Rocksoft Model CRC**.

- Also class provides bitwise shift method of CRC count. This method can calculate CRC with any bitness.

## Usage

Example:

```
Dim crcModel As New Crc.RocksoftCrcModel(CrcPresets.CRC16_CCITT)
Dim c As UInteger = crcModel.ComputeCrc(messageBytes)
```
