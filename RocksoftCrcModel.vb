Imports System.Collections
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq

Namespace Crc

    ''' <summary>
    ''' Реализует алгоритм расчёта CRC методом Rocksoft^tm Model CRC.
    ''' </summary>
    Public Class RocksoftCrcModel

#Region "NESTED TYPES"

        ''' <summary>
        ''' Стандартные алгоритмы расчёта контрольной суммы.
        ''' </summary>
        Public Enum CrcPresets As Integer
            CRC8
            CRC16
            CRC32
            CRC8_CDMA2000
            CRC8_DARC
            CRC8_DVB
            CRC8_EBU
            CRC8_ICODE
            CRC8_ITU
            CRC8_MAXIM
            CRC8_ROHC
            CRC8_WCDMA
            CRC16_AUG
            CRC16_BUYPASS
            CRC16_CCITT
            CRC16_CDMA2000
            CRC16_DDS
            CRC16_DECTR
            CRC16_DECTX
            CRC16_DNP
            CRC16_EN13757
            CRC16_GENIBUS
            CRC16_MAXIM
            CRC16_MCRF4XX
            CRC16_RIELLO
            CRC16_T10DIF
            CRC16_TELEDISK
            CRC16_TMS37157
            CRC16_USB
            CRC16_CRCA
            CRC16_KERMIT
            CRC16_MODBUS
            CRC16_X25
            CRC16_XMODEM
            CRC32_BZIP2
            CRC32_C
            CRC32_D
            CRC32_MPEG2
            CRC32_POSIX
            CRC32_Q
            CRC32_JAMCRC
            CRC32_XFER
        End Enum

        Public ReadOnly Property ProfileNames As Dictionary(Of CrcPresets, String)
            Get
                If (_ProfileNames.Count = 0) Then
                    For Each kvp In Profiles
                        _ProfileNames.Add(kvp.Key, Profiles(kvp.Key).Title)
                    Next
                End If
                Return _ProfileNames
            End Get
        End Property
        Private _ProfileNames As New Dictionary(Of CrcPresets, String)

        ''' <summary>
        ''' Список стандартных параметров для расчёта CRC.
        ''' </summary>
        Public ReadOnly Property Profiles As New Dictionary(Of CrcPresets, CrcParams) From {
            {CrcPresets.CRC8, New CrcParams("CRC-8", 8, &H7, 0, False, False, 0)},
            {CrcPresets.CRC16, New CrcParams("CRC-16/ARC", 16, &H8005, 0, True, True, 0)},
            {CrcPresets.CRC32, New CrcParams("CRC-32/zlib", 32, &H4C11DB7, &HFFFFFFFFUI, True, True, &HFFFFFFFFUI)},
            {CrcPresets.CRC8_CDMA2000, New CrcParams("CRC-8/CDMA2000", 8, &H9B, &HFF, False, False, 0)},
            {CrcPresets.CRC8_DARC, New CrcParams("CRC-8/DARC", 8, &H39, 0, True, True, 0)},
            {CrcPresets.CRC8_DVB, New CrcParams("CRC-8/DVB-S2", 8, &HD5, 0, False, False, 0)},
            {CrcPresets.CRC8_EBU, New CrcParams("CRC-8/EBU", 8, &H1D, &HFF, True, True, 0)},
            {CrcPresets.CRC8_ICODE, New CrcParams("CRC-8/I-CODE", 8, &H1D, &HFD, False, False, 0)},
            {CrcPresets.CRC8_ITU, New CrcParams("CRC-8/ITU", 8, &H7, 0, False, False, &H55)},
            {CrcPresets.CRC8_MAXIM, New CrcParams("CRC-8/MAXIM", 8, &H31, 0, True, True, 0)},
            {CrcPresets.CRC8_ROHC, New CrcParams("CRC-8/ROHC", 8, &H7, &HFF, True, True, 0)},
            {CrcPresets.CRC8_WCDMA, New CrcParams("CRC-8/WCDMA", 8, &H9B, 0, True, True, 0)},
            {CrcPresets.CRC16_AUG, New CrcParams("CRC-16/AUG-CCITT", 16, &H1021, &H1D0F, False, False, 0)},
            {CrcPresets.CRC16_BUYPASS, New CrcParams("CRC-16/BUYPASS", 16, &H8005, 0, False, False, 0)},
            {CrcPresets.CRC16_CCITT, New CrcParams("CRC-16/CCITT-FALSE", 16, &H1021, &HFFFF, False, False, 0)},
            {CrcPresets.CRC16_CDMA2000, New CrcParams("CRC-16/CDMA2000", 16, &HC867, &HFFFF, False, False, 0)},
            {CrcPresets.CRC16_DDS, New CrcParams("CRC-16/DDS-110", 16, &H8005, &H800D, False, False, 0)},
            {CrcPresets.CRC16_DECTR, New CrcParams("CRC-16/DECT-R", 16, &H589, 0, False, False, &H1)},
            {CrcPresets.CRC16_DECTX, New CrcParams("CRC-16/DECT-X", 16, &H589, 0, False, False, 0)},
            {CrcPresets.CRC16_DNP, New CrcParams("CRC-16/DNP", 16, &H3D65, 0, True, True, &HFFFF)},
            {CrcPresets.CRC16_EN13757, New CrcParams("CRC-16/EN-13757", 16, &H3D65, 0, False, False, &HFFFF)},
            {CrcPresets.CRC16_GENIBUS, New CrcParams("CRC-16/GENIBUS", 16, &H1021, &HFFFF, False, False, &HFFFF)},
            {CrcPresets.CRC16_MAXIM, New CrcParams("CRC-16/MAXIM", 16, &H8005, 0, True, True, &HFFFF)},
            {CrcPresets.CRC16_MCRF4XX, New CrcParams("CRC-16/MCRF4XX", 16, &H1021, &HFFFF, True, True, 0)},
            {CrcPresets.CRC16_RIELLO, New CrcParams("CRC-16/RIELLO", 16, &H1021, &HB2AA, True, True, 0)},
            {CrcPresets.CRC16_T10DIF, New CrcParams("CRC-16/T10-DIF", 16, &H8BB7, 0, False, False, 0)},
            {CrcPresets.CRC16_TELEDISK, New CrcParams("CRC-16/TELEDISK", 16, &HA097, 0, False, False, 0)},
            {CrcPresets.CRC16_TMS37157, New CrcParams("CRC-16/TMS37157", 16, &H1021, &H89EC, True, True, 0)},
            {CrcPresets.CRC16_USB, New CrcParams("CRC-16/USB", 16, &H8005, &HFFFF, True, True, &HFFFF)},
            {CrcPresets.CRC16_CRCA, New CrcParams("CRC-A", 16, &H1021, &HC6C6, True, True, 0)},
            {CrcPresets.CRC16_KERMIT, New CrcParams("CRC-16/KERMIT", 16, &H1021, 0, True, True, 0)},
            {CrcPresets.CRC16_MODBUS, New CrcParams("CRC-16/MODBUS", 16, &H8005, &HFFFF, True, True, 0)},
            {CrcPresets.CRC16_X25, New CrcParams("CRC-16/X-25", 16, &H1021, &HFFFF, True, True, &HFFFF)},
            {CrcPresets.CRC16_XMODEM, New CrcParams("CRC-16/XMODEM", 16, &H1021, 0, False, False, 0)},
            {CrcPresets.CRC32_BZIP2, New CrcParams("CRC-32/BZIP2", 32, &H4C11DB7, &HFFFFFFFFUI, False, False, &HFFFFFFFFUI)},
            {CrcPresets.CRC32_C, New CrcParams("CRC-32C", 32, &H1EDC6F41, &HFFFFFFFFUI, True, True, &HFFFFFFFFUI)},
            {CrcPresets.CRC32_D, New CrcParams("CRC-32D", 32, &HA833982BUI, &HFFFFFFFFUI, True, True, &HFFFFFFFFUI)},
            {CrcPresets.CRC32_MPEG2, New CrcParams("CRC-32/MPEG-2", 32, &H4C11DB7, &HFFFFFFFFUI, False, False, 0)},
            {CrcPresets.CRC32_POSIX, New CrcParams("CRC-32/POSIX", 32, &H4C11DB7, 0, False, False, &HFFFFFFFFUI)},
            {CrcPresets.CRC32_Q, New CrcParams("CRC-32Q", 32, &H814141ABUI, 0, False, False, 0)},
            {CrcPresets.CRC32_JAMCRC, New CrcParams("CRC-32/JAMCRC", 32, &H4C11DB7, &HFFFFFFFFUI, True, True, 0)},
            {CrcPresets.CRC32_XFER, New CrcParams("CRC-32/XFER", 32, &HAF, 0, False, False, 0)}
        }

        ''' <summary>
        ''' Контейнер для хранения настроек для расчёта CRC.
        ''' </summary>
        Public Class CrcParams

            Public Property Title As String
            Public Property Polynom As UInteger
            Public Property InitRegister As UInteger
            Public Property XorOut As UInteger
            Public Property CrcWidth As Integer
            Public Property ReflectIn As Boolean
            Public Property ReflectOut As Boolean

            Public Sub New(title As String, width As Integer, poly As UInteger, initReg As UInteger, refIn As Boolean, refOut As Boolean, xorOut As UInteger)
                Me.Title = title
                Me.CrcWidth = width
                Me.Polynom = poly
                Me.InitRegister = initReg
                Me.XorOut = xorOut
                Me.ReflectIn = refIn
                Me.ReflectOut = refOut
            End Sub

        End Class

#End Region '/NESTED TYPES

#Region "EVENTS"

        ''' <summary>
        ''' Уведомление об изменении параметров CRC, что должно служить триггером для пересчёта КС.
        ''' </summary>
        Public Event CrcParametersChanged()

#End Region '/EVENTS

#Region "PROPS AND FIELDS"

        ''' <summary>
        ''' Таблица предвычисленных значений для расчёта контрольной суммы.
        ''' </summary>
        Public ReadOnly CrcLookupTable(255) As UInteger

        ''' <summary>
        ''' Порядок CRC, в битах (8/16/32).
        ''' Изменение этого свойства ведёт к пересчёту таблицы.
        ''' </summary>
        Public Property CrcWidth As Integer
            Get
                Return _CrcWidth
            End Get
            Set(value As Integer)
                If (_CrcWidth <> value) Then
                    _CrcWidth = value
                    _StaticLastUsedCrcWidth = value
                    Polynom = Polynom And WidMask
                    InitRegister = InitRegister And WidMask
                    XorOut = XorOut And WidMask
                    GenerateLookupTable()
                    RaiseEvent CrcParametersChanged()
                End If
            End Set
        End Property
        Private _CrcWidth As Integer = 32
        Private Shared _StaticLastUsedCrcWidth As Integer = 32

        ''' <summary>
        ''' Последняя использованная разрядность контрольной суммы (статический метод!).
        ''' </summary>
        Public Shared Function GetLastUsedCrcBitness() As Integer
            Return _StaticLastUsedCrcWidth
        End Function

        ''' <summary>
        ''' Образующий многочлен.
        ''' </summary>
        ''' <remarks>
        ''' Это битовая величина, которая для удобства может быть представлена шестнадцатеричным числом. 
        ''' Старший бит при этом опускается. 
        ''' Например, если используется полином 10110, то он обозначается числом "06h". 
        ''' Важной особенностью данного параметра является то, что он всегда представляет собой необращенный полином, 
        ''' младшая часть этого параметра во время вычислений всегда является наименее значащими битами делителя вне зависимости от того, 
        ''' какой – "зеркальный" или прямой алгоритм моделируется.
        ''' Изменение этого свойства ведёт к пересчёту таблицы.
        ''' </remarks>
        Public Property Polynom As UInteger
            Get
                Return _Polynom
            End Get
            Set(value As UInteger)
                If (_Polynom <> value) Then
                    _Polynom = value
                    GenerateLookupTable()
                    RaiseEvent CrcParametersChanged()
                End If
            End Set
        End Property
        Private _Polynom As UInteger = &H4C11DB7

        ''' <summary>
        ''' Обращать байты сообщения?
        ''' </summary>
        ''' <remarks>
        ''' Логический параметр. Если он имеет значение "False" ("Ложь"), байты сообщения обрабатываются, начиная с 7 бита, 
        ''' который считается наиболее значащим, а наименее значащим считается бит 0. 
        ''' Если параметр имеет значение "True" ("Истина"), то каждый байт перед обработкой обращается.
        ''' Изменение этого свойства ведёт к пересчёту таблицы.
        ''' </remarks>
        Public Property ReflectIn As Boolean
            Get
                Return _ReflectIn
            End Get
            Set(value As Boolean)
                If (_ReflectIn <> value) Then
                    _ReflectIn = value
                    GenerateLookupTable()
                    RaiseEvent CrcParametersChanged()
                End If
            End Set
        End Property
        Private _ReflectIn As Boolean = True

        ''' <summary>
        ''' Начальное одержимое регситра.
        ''' </summary>
        ''' <remarks>
        ''' Определяет исходное содержимое регистра на момент запуска вычислений. 
        ''' Именно это значение должно быть занесено в регистр в прямой табличном алгоритме. 
        ''' В принципе, в табличных алгоритмах мы всегда может считать, 
        ''' что регистр инициализируется нулевым значением, а начальное значение комбинируется по Xor с содержимым регистра после N цикла. 
        ''' Данный параметр указывается шестнадцатеричным числом.
        ''' </remarks>
        Public Property InitRegister As UInteger
            Get
                Return _InitRegister
            End Get
            Set(value As UInteger)
                If (_InitRegister <> value) Then
                    _InitRegister = value
                    RaiseEvent CrcParametersChanged()
                End If
            End Set
        End Property
        Private _InitRegister As UInteger = &HFFFFFFFFUI

        ''' <summary>
        ''' Обращать выходное значение CRC?
        ''' </summary>
        ''' <remarks>
        ''' Логический параметр. Если он имеет значение "False" ("Ложь"), 
        ''' то конечное содержимое регистра сразу передается на стадию XorOut, 
        ''' в противном случае, когда параметр имеет значение "True" ("Истина"), 
        ''' содержимое регистра обращается перед передачей на следующую стадию вычислений.
        ''' </remarks>
        Public Property ReflectOut As Boolean
            Get
                Return _ReflectOut
            End Get
            Set(value As Boolean)
                If (_ReflectOut <> value) Then
                    _ReflectOut = value
                    RaiseEvent CrcParametersChanged()
                End If
            End Set
        End Property
        Private _ReflectOut As Boolean = True

        ''' <summary>
        ''' Значение, с которым XOR-ится выходное значение CRC.
        ''' </summary>
        ''' <remarks>
        ''' W битное значение, обозначаемое шестнадцатеричным числом. 
        ''' Оно комбинируется с конечным содержимым регистра (после стадии RefOut), 
        ''' прежде чем будет получено окончательное значение контрольной суммы.
        ''' </remarks>
        Public Property XorOut As UInteger
            Get
                Return _XorOut
            End Get
            Set(value As UInteger)
                If (_XorOut <> value) Then
                    _XorOut = value
                    RaiseEvent CrcParametersChanged()
                End If
            End Set
        End Property
        Private _XorOut As UInteger = &HFFFFFFFFUI

        ''' <summary>
        ''' Профиль с настройками CRC для некоторых популярных стандартных параметрических моделей.
        ''' </summary>
        Public Property Profile As CrcPresets
            Get
                Return _Profile
            End Get
            Set(value As CrcPresets)
                If (_Profile <> value) Then
                    If (Not Profiles.ContainsKey(value)) Then
                        Throw New NotImplementedException($"Профиль {value} не определён.")
                    End If
                    _Profile = value

                    Polynom = Profiles(value).Polynom
                    InitRegister = Profiles(value).InitRegister
                    XorOut = Profiles(value).XorOut
                    CrcWidth = Profiles(value).CrcWidth
                    ReflectIn = Profiles(value).ReflectIn
                    ReflectOut = Profiles(value).ReflectOut

                    RaiseEvent CrcParametersChanged()
                End If
            End Set
        End Property
        Private _Profile As CrcPresets = CrcPresets.CRC32

#End Region '/PROPS AND FIELDS

#Region "READ-ONLY PROPS"

        ''' <summary>
        ''' Возвращает длинное слово со значением (2^width)-1.
        ''' </summary>
        Private ReadOnly Property WidMask As UInteger
            Get
                Return (((1UI << (CrcWidth - 1)) - 1UI) << 1) Or 1UI
            End Get
        End Property

#End Region '/READ-ONLY PROPS

#Region "CTOR"

        ''' <summary>
        ''' Конструктор, инициализированный параметрами по умолчанию для алгоритма CRC32.
        ''' </summary>
        Public Sub New()
            GenerateLookupTable()
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RocksoftCrcModel"/> class.
        ''' </summary>
        ''' <param name="crcParamsModel">Заданная параметрическая модель CRC.</param>
        Public Sub New(crcParamsModel As CrcPresets)
            Me.Profile = crcParamsModel
        End Sub

        ''' <summary>
        ''' Инициализирует новый экземпляр параметрической модели CRC с настраиваемыми параметрами.
        ''' </summary>
        ''' <param name="width">Разрядность контрольной суммы в битах.</param>
        ''' <param name="poly">Полином.</param>
        ''' <param name="initReg">начальн7ое содержимое регистра.</param>
        ''' <param name="isReflectIn">Обращать ли входящие байты сообщения?</param>
        ''' <param name="isReflectOut">Обратить ли CRC перед финальным XOR.</param>
        ''' <param name="xorOut">Конечное значение XOR.</param>
        Public Sub New(width As Integer, poly As UInteger,
                       Optional initReg As UInteger = &HFFFFFFFFUI,
                       Optional isReflectIn As Boolean = True,
                       Optional isReflectOut As Boolean = True,
                       Optional xorOut As UInteger = &HFFFFFFFFUI)
            Me.CrcWidth = width
            Me.Polynom = poly
            Me.InitRegister = initReg
            Me.ReflectIn = isReflectIn
            Me.ReflectOut = isReflectOut
            Me.XorOut = xorOut
            GenerateLookupTable()
        End Sub

#End Region '/CTOR

#Region "ВЫЧИСЛЕНИЕ CRC"

        Public Function ComputeCrcByTable(ByRef message As Byte()) As UInteger 'TEST 
            Dim registerContent As UInteger = InitRegister 'Содержимое регистра в процессе пересчёта CRC.
            For Each b As Byte In message
                registerContent = GetNextRegisterContent(registerContent, b)
            Next
            Dim finalCrc As UInteger = GetFinalCrc(registerContent)
            Return finalCrc
        End Function

        ''' <summary>
        ''' Вычисляет значение контрольной суммы переданного сообщения.
        ''' </summary>
        ''' <param name="message">Исходное сообщение, для которого нужно посчитать контрольную сумму.</param>
        Public Function ComputeCrc(ByRef message As Byte()) As UInteger
            Dim registerContent As UInteger = InitRegister 'Содержимое регистра в процессе пересчёта CRC.
            For Each b As Byte In message
                registerContent = GetNextRegisterContent(registerContent, b)
            Next
            Dim finalCrc As UInteger = GetFinalCrc(registerContent)
            Return finalCrc
        End Function

        ''' <summary>
        ''' Вычисляет значение контрольной суммы переданного сообщения и возвращает его в виде массива байтов.
        ''' </summary>
        ''' <param name="message">Исходное сообщение, для которого нужно посчитать контрольную сумму.</param>
        Public Function ComputeCrcAsBytes(message As Byte()) As Byte()
            Dim crc As UInteger = ComputeCrc(message)
            Dim crcWidthInBytes As Integer = CrcWidth \ 8
            Dim crcBytes As IEnumerable(Of Byte) = GetBytesFromInt(crc).Skip(4 - crcWidthInBytes).Take(crcWidthInBytes) 'TEST Проверить для CRC8.
            Return crcBytes.ToArray()
        End Function

        ''' <summary>
        ''' Вычисляет значение контрольной суммы переданного сообщения и возвращает его в виде массива байтов.
        ''' </summary>
        ''' <param name="srcFilePath">Путь к файлу, для которого нужно рассчитать CRC.</param>
        Public Function ComputeCrcAsBytes(srcFilePath As String) As Byte()
            Dim registerContent As UInteger = InitRegister 'Содержимое регистра в процессе пересчёта CRC.
            Using sr As New IO.FileStream(srcFilePath, IO.FileMode.Open)
                Do While (sr.Position < sr.Length)
                    Try
                        Dim b As Integer = sr.ReadByte
                        registerContent = GetNextRegisterContent(registerContent, b)
                    Catch ex As Exception
                        Debug.WriteLine(ex.Message)
                    End Try
                Loop
            End Using
            Dim finalCrc As UInteger = GetFinalCrc(registerContent)
            Return GetBytesFromInt(finalCrc)
        End Function

        ''' <summary>
        ''' Обрабатывает один байт сообщения (0..255).
        ''' </summary>
        ''' <param name="prevRegContent">Содержимое регистра на предыдущем шаге.</param>
        ''' <param name="value">Значение очередного байта из сообщения.</param>
        Private Function GetNextRegisterContent(prevRegContent As UInteger, value As Integer) As UInteger
            Dim uValue As UInteger = CUInt(value)
            If ReflectIn Then
                uValue = Reflect(uValue, 8)
            End If
            Dim reg As UInteger = prevRegContent
            reg = reg Xor (uValue << (CrcWidth - 8))
            For i As Integer = 0 To 7
                If ((reg And TopBit) = TopBit) Then
                    reg = (reg << 1) Xor Polynom
                Else
                    reg <<= 1
                End If
                reg = reg And WidMask()
            Next
            Return reg
        End Function

        ''' <summary>
        ''' Возвращает значение CRC для обработанного сообщения.
        ''' </summary>
        ''' <param name="regContent">Значение регистра до финального обращения и XORа.</param>
        Private Function GetFinalCrc(regContent As UInteger) As UInteger
            If ReflectOut Then
                Dim res As UInteger = XorOut Xor Reflect(regContent, CrcWidth)
                Return res
            Else
                Dim res As UInteger = XorOut Xor regContent
                Return res
            End If
        End Function

        ''' <summary>
        ''' Возвращает старший разряд полинома.
        ''' </summary>
        Private Function TopBit() As UInteger
            Return GetBitMask(CrcWidth - 1)
        End Function

#End Region '/ВЫЧИСЛЕНИЕ CRC

#Region "РАСЧЁТ ТАБЛИЦЫ"

        ''' <summary>
        ''' Вычисляет таблицу предвычисленных значений для расчёта контрольной суммы.
        ''' </summary>
        Private Sub GenerateLookupTable()
            For i As Integer = 0 To 255
                CrcLookupTable(i) = GenerateTableItem(i)
            Next
        End Sub

        ''' <summary>
        ''' Рассчитывает один байт таблицы значений для расчёта контрольной суммы
        ''' по алгоритму Rocksoft^tm Model CRC Algorithm.
        ''' </summary>
        ''' <param name="index">Индекс записи в таблице, 0..255.</param>
        Private Function GenerateTableItem(index As Integer) As UInteger

            Dim inbyte As UInteger = CUInt(index)

            If ReflectIn Then
                inbyte = Reflect(inbyte, 8)
            End If

            Dim reg As UInteger = inbyte << (CrcWidth - 8)

            Dim msb As UInteger = TopBit
            For i As Integer = 0 To 7
                If ((reg And msb) = msb) Then
                    reg = (reg << 1) Xor Polynom
                Else
                    reg <<= 1
                End If
            Next

            If ReflectIn Then
                reg = Reflect(reg, CrcWidth)
            End If

            Dim res As UInteger = reg And WidMask
            Return res

        End Function

#End Region '/РАСЧЁТ ТАБЛИЦЫ

#Region "ВСПОМОГАТЕЛЬНЫЕ"

        ''' <summary>
        ''' Преобразует CRC из числа в массив байтов.
        ''' </summary>
        ''' <param name="crc"></param>
        Public Shared Function ConvertCrc(crc As UInteger, Optional width As Integer = 32) As Byte()
            Dim crcBytes As Byte() = BitConverter.GetBytes(crc)
            Select Case width
                Case 8
                    Return {crcBytes(0)}
                Case 16
                    Return {crcBytes(1), crcBytes(0)}
                Case Else
                    Return {crcBytes(3), crcBytes(2), crcBytes(1), crcBytes(0)}
            End Select
        End Function

        ''' <summary>
        ''' Преобразует CRC из массива байтов в число.
        ''' </summary>
        ''' <param name="crc"></param>
        Public Shared Function ConvertCrc(crc As Byte()) As UInteger
            Dim crcNumber As UInteger
            Select Case crc.Length
                Case 1
                    crcNumber = crc(0)
                Case 2
                    crcNumber = CUInt(crc(0)) << 8 Or crc(1)
                Case 3
                    crcNumber = CUInt(crc(0)) << 16 Or CUInt(crc(1)) << 8 Or crc(2)
                Case 4
                    crcNumber = CUInt(crc(0)) << 24 Or CUInt(crc(1)) << 16 Or CUInt(crc(2)) << 8 Or crc(3)
                Case Else
                    crcNumber = UInteger.MaxValue
            End Select
            Return crcNumber
        End Function

        ''' <summary>
        ''' Обращает заданное число младших битов переданного числа.
        ''' </summary>
        ''' <param name="value">Число, которое требуется обратить ("отзеркалить").</param>
        ''' <param name="bitsToReflect">Сколько младших битов числа обратить, 0..32.</param>
        ''' <remarks>Например: reflect(0x3E23, 3) == 0x3E26.</remarks>
        Private Shared Function Reflect(value As UInteger, Optional bitsToReflect As Integer = 32) As UInteger
            Dim t As UInteger = value
            Dim reflected As UInteger = value
            For i As Integer = 0 To bitsToReflect - 1
                Dim bm As UInteger = GetBitMask(bitsToReflect - 1 - i)
                If ((t And 1) = 1) Then
                    reflected = reflected Or bm
                Else
                    reflected = reflected And Not bm
                End If
                t >>= 1
            Next
            Return reflected
        End Function

        ''' <summary>
        ''' Возвращает наибольший разряд числа.
        ''' </summary>
        ''' <param name="number">Число, разрядность которого следует определить, степень двойки.</param>
        Private Shared Function GetBitMask(number As Integer) As UInteger
            Dim res As UInteger = (1UI << number)
            Return res
        End Function

        ''' <summary>
        ''' Возвращает массив байтов, полученный из переданного числа.
        ''' </summary>
        ''' <param name="int">Целое неотрицательное число.</param>
        ''' <remarks>Метод GetBytes() возвращает байты не в том порядке, который нужен.</remarks>
        Private Shared Function GetBytesFromInt(int As UInteger) As Byte()
            Dim bytes As Byte() = BitConverter.GetBytes(int)
            Dim bytesOrdered(bytes.Length - 1) As Byte
            For i As Integer = 0 To bytes.Length - 1
                bytesOrdered(i) = bytes(bytes.Length - 1 - i)
            Next
            Return bytesOrdered
        End Function

        Public Overrides Function ToString() As String
            Dim sb As New Text.StringBuilder()
            sb.Append($"{Polynom:X}, ")
            sb.Append($"{InitRegister:X}, ")
            sb.Append($"{XorOut:X}, ")
            sb.Append($"{ReflectIn:D1}, ")
            sb.Append($"{ReflectOut:D1}")
            Return sb.ToString()
        End Function

#End Region '/ВСПОМОГАТЕЛЬНЫЕ

#Region "CRC SIMPLE - РАСЧЁТ МЕТОДОМ ПОБИТОВОГО СДВИГА"

        ''' <summary>
        ''' Рассчитывает контрольную сумму типа CRC, рассчитанную методом побитового сдвига.
        ''' </summary>
        ''' <param name="bytes">Входная последовательность байтов (исходное сообщение).</param>
        ''' <param name="poly">Образующий многочлен разрядности <paramref name="width">width</paramref>.</param>
        ''' <param name="width">Порядок CRC в битах.</param>
        ''' <returns>По статье Ross N. Williams: "A Painless Guide to CRC Error Detection Algorithms".</returns>
        Public Shared Function ComputeCrcBitwise(bytes As Byte(), poly As UInteger,
                                                 Optional width As Integer = 32,
                                                 Optional initReg As UInteger = &HFFFFFFFFUI, Optional finalXor As UInteger = &HFFFFFFFFUI,
                                                 Optional reverseBytes As Boolean = True, Optional reverseCrc As Boolean = True) As UInteger

            Dim widthInBytes As Integer = width \ 8

            'Дополняем сообщение width нулями (расчёт в байтах):
            ReDim Preserve bytes(bytes.Length - 1 + widthInBytes)

            'Создаём очередь битов из сообщения:
            Dim msgFifo As New Queue(Of Boolean)(bytes.Length * 8 - 1)
            For Each b As Byte In bytes
                Dim ba As New BitArray({b})
                If reverseBytes Then
                    For i As Integer = 0 To 7
                        msgFifo.Enqueue(ba(i))
                    Next
                Else
                    For i As Integer = 7 To 0 Step -1
                        msgFifo.Enqueue(ba(i))
                    Next
                End If
            Next

            'Создаём очередь из битов начального заполнения регистра:
            Dim initBytes As Byte() = BitConverter.GetBytes(initReg)
            Dim initBytesReversed As IEnumerable(Of Byte) = (From b As Byte In initBytes Take widthInBytes).Reverse
            Dim initFifo As New Queue(Of Boolean)(width - 1)
            For Each b As Byte In initBytesReversed
                Dim ba As New BitArray({b})
                If (Not reverseBytes) Then
                    For i As Integer = 0 To 7
                        initFifo.Enqueue(ba(i))
                    Next
                Else
                    For i As Integer = 7 To 0 Step -1
                        initFifo.Enqueue(ba(i))
                    Next
                End If
            Next

            'Сдвиг и XOR:
            Dim register As UInteger = 0 'заполняем width-разрядный регистр нулями.
            Do While (msgFifo.Count > 0)

                Dim poppedBit As Integer = CInt(register >> (width - 1)) And 1 'определить перед сдвигом регистра.

                Dim shiftedBit As Byte = Convert.ToByte(msgFifo.Dequeue)
                If (initFifo.Count > 0) Then
                    Dim b As Byte = Convert.ToByte(initFifo.Dequeue)
                    shiftedBit = shiftedBit Xor b
                End If

                register <<= 1
                register = register Or shiftedBit

                If (poppedBit = 1) Then
                    register = register Xor poly
                End If
            Loop

            'Финальные преобразования:s
            Dim crc As UInteger = register 'Регистр содержит остаток от деления == контрольную сумму.
            If reverseCrc Then
                crc = Reflect(crc, width)
            End If
            crc = crc Xor finalXor

            'Учитываем разрядность числа:
            Dim mask As Integer = 32 - width 'NOTE Если CRC - 32-разрядное, то ничего не меняется. Но если 16-ти или меньше, то этот вариант используется.
            crc = crc And (&HFFFFFFFFUI >> mask)

            Return crc

        End Function '/ComputeCrcBitwise

        ''' <summary>
        ''' Рассчитывает контрольную сумму типа CRC, рассчитанную методом побитового сдвига.
        ''' </summary>
        ''' <param name="ba">Входная последовательность битов (исходное сообщение).</param>
        ''' <param name="registerBitness">Разрядность информационного символа.</param>
        ''' <param name="poly">Образующий многочлен разрядности <paramref name="crcWidth">width</paramref>.</param>
        ''' <param name="crcWidth">Порядок CRC в битах.</param>
        ''' <returns>По статье Ross N. Williams: "A Painless Guide to CRC Error Detection Algorithms".</returns>
        Public Shared Function ComputeCrcBitwise(ba As BitArray, registerBitness As Integer, poly As UInteger,
                                                 Optional crcWidth As Integer = 32,
                                                 Optional initReg As UInteger = &HFFFFFFFFUI, Optional finalXor As UInteger = &HFFFFFFFFUI,
                                                 Optional reverseBytes As Boolean = True, Optional reverseCrc As Boolean = True) As UInteger

            'Создаём очередь битов из сообщения:
            Dim msgFifo As New Queue(Of Boolean)
            For n As Integer = 0 To ba.Length - 1 Step 11
                If reverseBytes Then
                    For i As Integer = registerBitness - 1 To 0 Step -1
                        Dim v As Boolean = ba(n + i)
                        msgFifo.Enqueue(v)
                    Next
                Else
                    For i As Integer = 0 To registerBitness - 1
                        Dim v As Boolean = ba(n + i)
                        msgFifo.Enqueue(v)
                    Next
                End If
            Next

            'Дополняем сообщение crcWidth нулями:
            For i As Integer = 0 To crcWidth - 1
                msgFifo.Enqueue(False)
            Next

            'Создаём очередь из битов начального заполнения регистра:
            Dim initBytes As Byte() = BitConverter.GetBytes(initReg)
            Dim widthInBytes As Integer = crcWidth \ 8
            Dim initBytesReversed As IEnumerable(Of Byte) = (From b As Byte In initBytes Take widthInBytes).Reverse
            Dim initFifo As New Queue(Of Boolean)(crcWidth - 1)
            For Each b As Byte In initBytesReversed
                Dim bar As New BitArray({b})
                If (Not reverseBytes) Then
                    For i As Integer = 0 To 7
                        initFifo.Enqueue(bar(i))
                    Next
                Else
                    For i As Integer = 7 To 0 Step -1
                        initFifo.Enqueue(bar(i))
                    Next
                End If
            Next

            'Сдвиг и XOR:
            Dim registerWidthLimit As UInteger = GetBitMask(crcWidth) - 1UI 'ограничитель разрядности регистра
            Dim register As UInteger = 0 'заполняем crcWidth-разрядный регистр нулями.
            Do While (msgFifo.Count > 0)
                Dim poppedBit As Integer = CInt(register >> (crcWidth - 1)) And 1 'определяем выдвигаемый из регистра бит перед сдвигом сообщения.
                Dim shiftedBit As Byte = Convert.ToByte(msgFifo.Dequeue()) 'вдвигаемый бит

                If (initFifo.Count > 0) Then
                    Dim b As Byte = Convert.ToByte(initFifo.Dequeue)
                    shiftedBit = shiftedBit Xor b
                End If

                register = (register << 1) And registerWidthLimit 'сдвигаем регистр и ограничиваем разрядностью crcWidth
                register = register Or shiftedBit 'вдвигаем в регистр бит сообщения

                If (poppedBit = 1) Then
                    register = register Xor poly
                End If
            Loop

            'Финальные преобразования:
            Dim crc As UInteger = register 'Регистр содержит остаток от деления == контрольную сумму.
            If reverseCrc Then
                crc = Reflect(crc, crcWidth)
            End If
            crc = crc Xor finalXor

            'Учитываем разрядность числа:
            Dim mask As Integer = 32 - crcWidth 'NOTE Если CRC - 32-разрядное, то ничего не меняется. Но если 16-ти или меньше, то этот вариант используется.
            crc = crc And (&HFFFFFFFFUI >> mask)

            Return crc

        End Function '/ComputeCrcBitwise

#End Region '/CRC SIMPLE - РАСЧЁТ МЕТОДОМ ПОБИТОВОГО СДВИГА"


    End Class '/Crc.RocksoftCrc

End Namespace
