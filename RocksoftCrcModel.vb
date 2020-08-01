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
        Public Enum CrcPresetEnum As Integer
            CRC8
            CRC16
            CRC_CITT
            XMODEM
            CRC32
        End Enum

#End Region '/NESTED TYPES

        <Obsolete("То же, что и CrcWidth, только статическое. Использовать с опаской, т.к. небезопасно.")>
        Public Shared ReadOnly Property CrcBitness As Integer
            Get
                Return _CrcWidth
            End Get
        End Property

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
                    _TopBit = GetBitMask(CrcWidth - 1)
                    _WidMask = (((1UI << (CrcWidth - 1)) - 1UI) << 1) Or 1UI
                    Polynom = Polynom And WidMask
                    InitRegister = InitRegister And WidMask
                    XorOut = XorOut And WidMask
                    GenerateLookupTable()
                End If
            End Set
        End Property
        Private Shared _CrcWidth As Integer = 32

        ''' <summary>
        ''' Образующий многочлен.
        ''' Изменение этого свойства ведёт к пересчёту таблицы.
        ''' </summary>
        Public Property Polynom As UInteger
            Get
                Return _Polynom
            End Get
            Set(value As UInteger)
                If (_Polynom <> value) Then
                    _Polynom = value
                    GenerateLookupTable()
                End If
            End Set
        End Property
        Private _Polynom As UInteger = &H4C11DB7

        ''' <summary>
        ''' Обращать байты сообщения?
        ''' Изменение этого свойства ведёт к пересчёту таблицы.
        ''' </summary>
        Public Property ReflectIn As Boolean
            Get
                Return _ReflectIn
            End Get
            Set(value As Boolean)
                If (_ReflectIn <> value) Then
                    _ReflectIn = value
                    GenerateLookupTable()
                End If
            End Set
        End Property
        Private _ReflectIn As Boolean = True

        ''' <summary>
        ''' Начальное одержимое регситра.
        ''' </summary>
        Public Property InitRegister As UInteger
            Get
                Return _InitRegister
            End Get
            Set(value As UInteger)
                If (_InitRegister <> value) Then
                    _InitRegister = value
                End If
            End Set
        End Property
        Private _InitRegister As UInteger = &HFFFFFFFFUI

        ''' <summary>
        ''' Обращать выходное значение CRC?
        ''' </summary>
        Public Property ReflectOut As Boolean
            Get
                Return _ReflectOut
            End Get
            Set(value As Boolean)
                If (_ReflectOut <> value) Then
                    _ReflectOut = value
                End If
            End Set
        End Property
        Private _ReflectOut As Boolean = True

        ''' <summary>
        ''' Значение, с которым XOR-ится выходное значение CRC.
        ''' </summary>
        Public Property XorOut As UInteger
            Get
                Return _XorOut
            End Get
            Set(value As UInteger)
                If (_XorOut <> value) Then
                    _XorOut = value
                End If
            End Set
        End Property
        Private _XorOut As UInteger = &HFFFFFFFFUI

        ''' <summary>
        ''' Профиль с настройками CRC для некоторых популярных стандартных параметрических моделей.
        ''' </summary>
        Public Property Profile As CrcPresetEnum
            Get
                Return _Profile
            End Get
            Set(value As CrcPresetEnum)
                If (_Profile <> value) Then
                    _Profile = value
                    Select Case value
                        Case CrcPresetEnum.CRC32
                            Polynom = &H4C11DB7
                            InitRegister = &HFFFFFFFFUI
                            XorOut = &HFFFFFFFFUI
                            CrcWidth = 32
                            ReflectIn = True
                            ReflectOut = True
                        Case CrcPresetEnum.CRC16
                            Polynom = &H8005
                            InitRegister = 0
                            XorOut = 0
                            CrcWidth = 16
                            ReflectIn = True
                            ReflectOut = True
                        Case CrcPresetEnum.CRC_CITT
                            Polynom = &H1021
                            InitRegister = &HFFFF
                            XorOut = 0
                            CrcWidth = 16
                            ReflectIn = False
                            ReflectOut = False
                        Case CrcPresetEnum.XMODEM
                            Polynom = &H8408
                            InitRegister = 0
                            XorOut = 0
                            CrcWidth = 16
                            ReflectIn = True
                            ReflectOut = True
                        Case CrcPresetEnum.CRC8
                            Polynom = &H7
                            InitRegister = 0
                            XorOut = 0
                            CrcWidth = 8
                            ReflectIn = False
                            ReflectOut = False
                        Case Else
                            Throw New NotImplementedException("Профиль не определён")
                    End Select
                End If
            End Set
        End Property
        Private _Profile As CrcPresetEnum = CrcPresetEnum.CRC32

#End Region '/PROPS AND FIELDS

#Region "READ-ONLY PROPS"

        ''' <summary>
        ''' Возвращает старший разряд полинома.
        ''' </summary>
        ReadOnly Property TopBit As UInteger
            Get
                Return _TopBit
                'Return getBitMask(CrcWidth - 1) 'рассчитываем один раз при изменении порядка полинома.
            End Get
        End Property
        Private _TopBit As UInteger = GetBitMask(CrcWidth - 1)

        ''' <summary>
        ''' Возвращает длинное слово со значением (2^width)-1.
        ''' </summary>
        Private ReadOnly Property WidMask As UInteger
            Get
                Return _WidMask
            End Get
        End Property
        Private _WidMask As UInteger = (((1UI << (CrcWidth - 1)) - 1UI) << 1) Or 1UI

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
        Public Sub New(crcParamsModel As CrcPresetEnum)
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
        Public Sub New(ByVal width As Integer, ByVal poly As UInteger,
                       Optional ByVal initReg As UInteger = &HFFFFFFFFUI,
                       Optional ByVal isReflectIn As Boolean = True,
                       Optional ByVal isReflectOut As Boolean = True,
                       Optional ByVal xorOut As UInteger = &HFFFFFFFFUI)
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
        Private Function GenerateTableItem(ByVal index As Integer) As UInteger

            Dim inbyte As UInteger = CUInt(index)

            If ReflectIn Then
                inbyte = Reflect(inbyte, 8)
            End If

            Dim reg As UInteger = inbyte << (CrcWidth - 8)

            For i As Integer = 0 To 7
                If ((reg And TopBit) = TopBit) Then
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
        Public Shared Function ConvertCrc(ByVal crc As UInteger, Optional ByVal width As Integer = 32) As Byte()
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
        Public Shared Function ConvertCrc(ByVal crc As Byte()) As UInteger
            Dim crcNumber As UInteger = 0
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
        Private Shared Function Reflect(ByVal value As UInteger, Optional ByVal bitsToReflect As Integer = 32) As UInteger
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
        Private Shared Function GetBitMask(ByVal number As Integer) As UInteger
            Dim res As UInteger = (1UI << number)
            Return res
        End Function

        ''' <summary>
        ''' Возвращает массив байтов, полученный из переданного числа.
        ''' </summary>
        ''' <param name="int">Целое неотрицательное число.</param>
        ''' <remarks>Метод GetBytes() возвращает байты не в том порядке, который нужен.</remarks>
        Private Shared Function GetBytesFromInt(ByVal int As UInteger) As Byte()
            Dim bytes As Byte() = BitConverter.GetBytes(int)
            Dim bytesOrdered(bytes.Length - 1) As Byte
            For i As Integer = 0 To bytes.Length - 1
                bytesOrdered(i) = bytes(bytes.Length - 1 - i)
            Next
            Return bytesOrdered
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
        Public Shared Function ComputeCrcBitwise(ByVal bytes As Byte(), ByVal poly As UInteger,
                                                 Optional ByVal width As Integer = 32,
                                                 Optional ByVal initReg As UInteger = &HFFFFFFFFUI, Optional ByVal finalXor As UInteger = &HFFFFFFFFUI,
                                                 Optional ByVal reverseBytes As Boolean = True, Optional ByVal reverseCrc As Boolean = True) As UInteger

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

                register = register << 1
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
        Public Shared Function ComputeCrcBitwise(ByVal ba As BitArray, ByVal registerBitness As Integer, ByVal poly As UInteger,
                                                 Optional ByVal crcWidth As Integer = 32,
                                                 Optional ByVal initReg As UInteger = &HFFFFFFFFUI, Optional ByVal finalXor As UInteger = &HFFFFFFFFUI,
                                                 Optional ByVal reverseBytes As Boolean = True, Optional ByVal reverseCrc As Boolean = True) As UInteger

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