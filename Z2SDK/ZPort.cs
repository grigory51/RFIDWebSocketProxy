using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ZPort {
    #region типы
    // Типы считывателей
    public enum ZP_PORT_TYPE {
        ZP_PORT_UNDEF = 0,
        ZP_PORT_COM,        // Com-порт
        ZP_PORT_FT,         // Ft-порт (через ftd2xx.dll по с/н USB)
        ZP_PORT_IP,         // Ip-порт (TCP-клиент)
        ZP_PORT_IPS         // Ip-порт (TCP-сервер)
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZP_ENUMPORTSPROC(ref ZP_PORT_INFO pInfo, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZP_NOTIFYPROC(UInt32 nMsg, IntPtr nMsgParam, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZP_ENUMDEVICEPROC(IntPtr pInfo, ref ZP_PORT_INFO pPort, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZP_DEVICEPARSEPROC([In] Byte[] pReply, IntPtr nCount, ref int nError, IntPtr pInfo, ref ZP_PORT_INFO pPort,
            ZP_ENUMDEVICEPROC fnEnum, IntPtr pEnumParam);
    #endregion

    #region структуры

    // Информация о порте
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_PORT_INFO {
        public ZP_PORT_TYPE nType;      // Тип порта
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szName;           // Имя порта
        public UInt32 nFlags;           // Флаги порта (ZPIntf.ZP_PF_BUSY,ZP_PF_USER,ZP_PF_BUSY2)
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szFriendly;       // Дружественное имя порта
        public UInt32 nDevTypes;        // Маска типов устройств
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szOwner;          // Владелец порта (для функции ZP_EnumIpDevices)
    }
    // Настройки ожидания исполнения функций
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ZP_WAIT_SETTINGS {
        public UInt32 nReplyTimeout;                   // Тайм-аут ожидания ответа на запрос конвертеру
        public int nMaxTry;                            // Количество попыток отправить запрос
        public IntPtr hAbortEvent;                     // Дескриптор стандартного объекта Event для прерывания функции. Если объект установлен в сигнальное состояние, функция возвращает E_ABORT
        public UInt32 nReplyTimeout0;                  // Тайм-аут ожидания первого символа ответа
        public UInt32 nCheckPeriod;                    // Период проверки порта в мс (если =0 или =INFINITE, то по RX-событию)

        public ZP_WAIT_SETTINGS(UInt32 _nReplyTimeout, int _nMaxTry, IntPtr _hAbortEvent, UInt32 _nReplyTimeout0, UInt32 _nCheckPeriod) {
            nReplyTimeout = _nReplyTimeout;
            nMaxTry = _nMaxTry;
            hAbortEvent = _hAbortEvent;
            nReplyTimeout0 = _nReplyTimeout0;
            nCheckPeriod = _nCheckPeriod;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_DEVICE_INFO {
        public UInt32 nTypeId;
        public UInt32 nModel;
        public UInt32 nSn;
        public UInt32 nVersion;
    }
    // Информация о порте
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_N_EXIST_INFO {
        public ZP_PORT_INFO rPort;
        [MarshalAs(UnmanagedType.LPStruct)]
        public ZP_DEVICE_INFO pInfo;
    }
    // Информация об изменении параметров порта
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_N_CHANGE_INFO {
        public UInt32 nChangeMask;     // Маска изменений
        public ZP_PORT_INFO rPort;
        public ZP_PORT_INFO rOldPort;
        [MarshalAs(UnmanagedType.LPStruct)]
        public ZP_DEVICE_INFO pInfo;
        [MarshalAs(UnmanagedType.LPStruct)]
        public ZP_DEVICE_INFO pOldInfo;
    }
    //[Obsolete("use ZP_N_CHANGE_INFO")]
    //ZP_N_CHANGE_STATE = ZP_N_CHANGE_INFO;
    // Параметры для уведомлений
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZP_NOTIFY_SETTINGS {
        public UInt32 nNMask;                           // Маска типов уведомлений (см. _ZP_NOTIFY_SETTINGS в ZPort.h)
        public ZPort.ZP_NOTIFYPROC pfnCallback;         // Callback-функция
        public IntPtr pUserData;                        // Параметр для Callback-функции
        public UInt32 nSDevTypes;                       // Маска тивов устройств, подключенных к последовательному порту
        public UInt32 nIpDevTypes;                      // Маска тивов Ip-устройств
        public IntPtr hSvcStatus;                       // Дескриптор сервиса, полученный функцией 
        public UInt32 nCheckUsbPeriod;                  // Период проверки состояния USB-портов (в миллисекундах) (=0 по умолчанию 5000)
        public UInt32 nCheckIpPeriod;                   // Период проверки состояния IP-портов (в миллисекундах) (=0 по умолчанию 15000)
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_DEVICE {
        public UInt32 nTypeId;                          // Тип устройства
        [MarshalAs(UnmanagedType.LPArray)]
        public Byte[] pReqData;                         // Данные запроса (может быть NULL)
        public IntPtr nReqSize;                         // Количество байт в запросе
        public ZP_DEVICEPARSEPROC pfnParse;             // Функция разбора ответа
        public IntPtr nDevInfoSize;                     // Размер структуры ZP_DEVICE_INFO
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_IP_DEVICE {
        public ZP_DEVICE rBase;
        public IntPtr nReqPort;                         // Порт для запроса
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_S_DEVICE {
        public ZP_DEVICE rBase;
        [MarshalAs(UnmanagedType.LPArray)]
        public UInt16[] pPids;                          // Pid'ы USB-устройств
        public int nPidCount;                           // Количество Pid'ов
        public UInt32 nBaud;                            // Скорость порта
        public SByte chEvent;                           // Символ-признак конца передачи (если =0, нет символа)
        public Byte nStopBits;                          // Стоповые биты (ONESTOPBIT=0, ONE5STOPBITS=1, TWOSTOPBITS=2)
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszBDesc;                         // Описание устройства, предоставленное шиной (DEVPKEY_Device_BusReportedDeviceDesc)
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_PORT_ADDR {
        public ZP_PORT_TYPE nType;                      // Тип порта
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pName;                            // Имя порта
        public UInt32 nDevTypes;                        // Маска тивов устройств
    }
    #endregion

    class ZPIntf {
        #region Константы: версия SDK
        public const int ZP_SDK_VER_MAJOR = 1;
        public const int ZP_SDK_VER_MINOR = 12;
        #endregion

        #region Константы кодов ошибок
        public const int S_OK = 0;                                            // Операция выполнена успешно
        public const int E_FAIL = unchecked((int)0x80000008);                 // Другая ошибка
        public const int E_OUTOFMEMORY = unchecked((int)0x80000002);          // Недостаточно памяти для обработки команды
        public const int E_INVALIDARG = unchecked((int)0x80000003);           // Неправильный параметр
        public const int E_NOINTERFACE = unchecked((int)0x80000004);          // Функция не поддерживается
        public const int E_ABORT = unchecked((int)0x80000007);                // Функция прервана (см.описание ZP_WAIT_SETTINGS)

        public const int ZP_S_CANCELLED = unchecked((int)0x00040201);         // Отменено пользователем
        public const int ZP_S_NOTFOUND = unchecked((int)0x00040202);          // Не найден (для функции ZP_FindSerialDevice)
        public const int ZP_E_OPENNOTEXIST = unchecked((int)0x80040203);      // Порт не существует
        public const int ZP_E_OPENACCESS = unchecked((int)0x80040204);        // Порт занят другой программой
        public const int ZP_E_OPENPORT = unchecked((int)0x80040205);          // Другая ошибка открытия порта
        public const int ZP_E_PORTIO = unchecked((int)0x80040206);            // Ошибка порта (Конвертор отключен от USB?)
        public const int ZP_E_PORTSETUP = unchecked((int)0x80040207);         // Ошибка настройки порта
        public const int ZP_E_LOADFTD2XX = unchecked((int)0x80040208);        // Неудалось загрузить FTD2XX.DLL
        public const int ZP_E_SOCKET = unchecked((int)0x80040209);            // Не удалось инициализировать сокеты
        public const int ZP_E_SERVERCLOSE = unchecked((int)0x8004020A);       // Дескриптор закрыт со стороны Сервера
        public const int ZP_E_NOTINITALIZED = unchecked((int)0x8004020B);     // Не проинициализировано с помощью ZP_Initialize
        public const int ZP_E_INSUFFICIENTBUFFER = unchecked((int)0x8004020C);// Размер буфера слишком мал
        [Obsolete("use S_OK")]
        public const int ZP_SUCCESS = S_OK;
        [Obsolete("use ZP_S_CANCELLED")]
        public const int ZP_E_CANCELLED = ZP_S_CANCELLED;
        [Obsolete("use ZP_S_NOTFOUND")]
        public const int ZP_E_NOT_FOUND = ZP_S_NOTFOUND;
        [Obsolete("use E_INVALIDARG")]
        public const int ZP_E_INVALID_PARAM = E_INVALIDARG;
        [Obsolete("use ZP_E_OPENNOTEXIST")]
        public const int ZP_E_OPEN_NOT_EXIST = ZP_E_OPENNOTEXIST;
        [Obsolete("use ZP_E_OPENACCESS")]
        public const int ZP_E_OPEN_ACCESS = ZP_E_OPENACCESS;
        [Obsolete("use ZP_E_OPENPORT")]
        public const int ZP_E_OPEN_PORT = ZP_E_OPENPORT;
        [Obsolete("use ZP_E_PORTIO")]
        public const int ZP_E_PORT_IO_ERROR = ZP_E_PORTIO;
        [Obsolete("use ZP_E_PORTSETUP")]
        public const int ZP_E_PORT_SETUP = ZP_E_PORTSETUP;
        [Obsolete("use ZP_E_LOADFTD2XX")]
        public const int ZP_E_LOAD_FTD2XX = ZP_E_LOADFTD2XX;
        [Obsolete("use ZP_E_INIT_SOCKET")]
        public const int ZP_E_INIT_SOCKET = ZP_E_SOCKET;
        [Obsolete("use E_OUTOFMEMORY")]
        public const int ZP_E_NOT_ENOUGH_MEMORY = E_OUTOFMEMORY;
        [Obsolete("use E_NOINTERFACE")]
        public const int ZP_E_UNSUPPORT = E_NOINTERFACE;
        [Obsolete("use ZP_E_NOTINITALIZED")]
        public const int ZP_E_NOT_INITALIZED = ZP_E_NOTINITALIZED;
        [Obsolete("use E_FAIL")]
        public const int ZP_E_CREATE_EVENT = E_FAIL;           // Ошибка функции CreateEvent
        [Obsolete("use E_FAIL")]
        public const int ZP_E_OTHER = E_FAIL;
        #endregion

        #region Константы ZP_Initialize
        // ZP_Initialize Flags
        public const uint ZP_IF_NO_MSG_LOOP = 0x01;     // Приложение не имеет цикла обработки сообщений (Console or Service)
        public const uint ZP_IF_ERROR_LOG = 0x02;       // Писать лог
        #endregion

        #region Константы для уведомлений
        public const uint ZP_NF_EXIST = 0x01;         // Уведомления о подключении/отключении порта (ZP_N_INSERT / ZP_N_REMOVE)
        public const uint ZP_NF_CHANGE = 0x02;        // Уведомление о изменении параметров порта (ZP_N_STATE_CHANGED)
        public const uint ZP_NF_ERROR = 0x08;         // Уведомление об ошибке в ните(thread), сканирующей порты (ZP_N_ERROR)
        public const uint ZP_NF_SDEVICE = 0x10;       // Информация о устройствах, подключенным к последовательным портам
        public const uint ZP_NF_USEVCOM = 0x2000;     // По возможности использовать Com-порт
        public const uint ZP_NF_WND_SYNC = 0x4000;    // Синхронизировать с очередью сообщений приложения
        public const uint ZP_NF_ONLY_NOTIFY = 0x8000; // Только уведомлять о добавлении новых сообщений в очередь (для перечисления и обработки сообщений используйте функцию ZP_ProcessMessages)
        [Obsolete("use ZP_NF_CHANGE")]
        public const uint ZP_NF_BUSY = ZP_NF_CHANGE;
        [Obsolete("use ZP_NF_CHANGE")]
        public const uint ZP_NF_FRIENDLY = ZP_NF_CHANGE;

        public const uint ZP_N_INSERT = 1;            // Подключение порта (ZP_N_EXIST_INFO(MsgParam) - инфо о порте)
        public const uint ZP_N_REMOVE = 2;            // Отключение порта (ZP_N_EXIST_INFO(MsgParam) - инфо о порте)
        public const uint ZP_N_CHANGE = 3;            // Изменение состояния порта (ZP_N_CHANGE_STATE(MsgParam) - инфо об изменениях)
        public const uint ZP_N_ERROR = 4;             // Произошла ошибка в ните (PHRESULT(MsgParam) - код ошибки)

        [Obsolete("use ZP_N_CHANGE")]
        public const uint ZP_N_STATE_CHANGED = ZP_N_CHANGE;

        // Флаги для ZP_N_CHANGE_INFO.nChangeMask
        public const uint ZP_CIF_BUSY = 4;
        public const uint ZP_CIF_FRIENDLY = 8;
        public const uint ZP_CIF_DEVTYPES = 0x10;
        public const uint ZP_CIF_OWNER = 0x20;
        public const uint ZP_CIF_MODEL = 0x80;
        public const uint ZP_CIF_SN = 0x100;
        public const uint ZP_CIF_VERSION = 0x200;
        public const uint ZP_CIF_DEVPARAMS = 0x400;
        #endregion

        #region Константы (Флаги порта)
        public const uint ZP_PF_BUSY = 1;   // Порт занят
        public const uint ZP_PF_USER = 2;   // Порт, указанный пользователем (массив ZP_PORT_ADDR)
        public const uint ZP_PF_BUSY2 = 4;  // Дружественный порт занят (Friendly)
        #endregion

        #region Константы (Флаги для функций ZP_EnumSerialDevices, ZP_FindSerialDevice и ZP_EnumIpDevices)
        public const uint ZP_SF_UPDATE = 1;     // Обновить список сейчас
        public const uint ZP_SF_USEVCOM = 2;    // По возможности использовать Com-порт
        #endregion

        //public const string ZpDllName = "ZPort.dll";
        public const string ZpDllName = "ZReader.dll";
        //public const string ZpDllName = "ZGuard.dll";

        //Функции библиотеки
        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetVersion")]
        public static extern UInt32 ZP_GetVersion();

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetFtLibVersion")]
        public static extern int ZP_GetFtLibVersion(ref UInt32 pVersion);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Initialize")]
        public static extern int ZP_Initialize(UInt32 nFlags);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Finalyze")]
        public static extern int ZP_Finalyze();

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_CloseHandle")]
        public static extern int ZP_CloseHandle(IntPtr hHandle);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_EnumSerialPorts")]
        public static extern int ZP_EnumSerialPorts(UInt32 nDevTypes, ZP_ENUMPORTSPROC pEnumProc, IntPtr pUserData);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_FindNotification")]
        public static extern int ZP_FindNotification(ref IntPtr pHandle, ref ZP_NOTIFY_SETTINGS pSettings);

        [Obsolete("use ZP_CloseHandle")]
        public static int ZP_CloseNotification(IntPtr hHandle) {
            return ZP_CloseHandle(hHandle);
        }

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_ProcessMessages")]
        public static extern int ZP_ProcessMessages(IntPtr hHandle, ZP_NOTIFYPROC pEnumProc, IntPtr pUserData);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_DeviceEventNotify")]
        public static extern void ZP_DeviceEventNotify(UInt32 nEvType, IntPtr pEvData);

        [DllImport(ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Open")]
        public static extern int ZP_Open(ref IntPtr pHandle, string sName, ZP_PORT_TYPE nType, UInt32 nBaud, char nEvChar, Byte nStopBits);

        [Obsolete("use ZP_CloseHandle")]
        public static int ZP_Close(IntPtr hHandle) {
            return ZP_CloseHandle(hHandle);
        }

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetBaudAndEvChar")]
        public static extern int ZP_GetBaudAndEvChar(IntPtr pHandle, ref UInt32 nBaud, ref char nEvChar);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetFtDriverVersion")]
        public static extern int ZP_GetFtDriverVersion(IntPtr pHandle, ref UInt32 pVersion);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Clear")]
        public static extern int ZP_Clear(IntPtr pHandle, bool fIn, bool fOut);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Write")]
        public static extern int ZP_Write(IntPtr hHandle, [In] Byte[] pData);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Read")]
        public static extern int ZP_Read(IntPtr hHandle, [Out] Byte[] pData, int nCount, ref int nRead);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetInCount")]
        public static extern int ZP_GetInCount(IntPtr hHandle, ref int nCount);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_StartWaitEvent")]
        public static extern int ZP_StartWaitEvent(IntPtr hHandle, ref IntPtr pEvent);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_SetDtr")]
        public static extern int ZP_SetDtr(IntPtr hHandle, bool fState);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_SetRts")]
        public static extern int ZP_SetRts(IntPtr hHandle, bool fState);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_RegSerialDevice")]
        public static extern int ZP_RegSerialDevice(ref ZP_S_DEVICE pParams);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_EnumSerialDevices")]
        public static extern int ZP_EnumSerialDevices(UInt32 nTypeMask,
            [In] [MarshalAs(UnmanagedType.LPArray)] ZP_PORT_ADDR[] pPorts, int nPCount,
            ZP_ENUMDEVICEPROC pEnumProc, IntPtr pUserData,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, UInt32 nFlags = (ZP_SF_UPDATE|ZP_SF_USEVCOM));

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_FindSerialDevice")]
        public static extern int ZP_FindSerialDevice(UInt32 nTypeMask,
            [In] [MarshalAs(UnmanagedType.LPArray)] ZP_PORT_ADDR[] pPorts, int nPCount,
            IntPtr pInfo, int nInfoSize, ref ZP_PORT_INFO pPort,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, UInt32 nFlags = (ZP_SF_UPDATE|ZP_SF_USEVCOM));

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_RegIpDevice")]
        public static extern int ZP_RegIpDevice(ref ZP_IP_DEVICE pParams);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_EnumIpDevices")]
        public static extern int ZP_EnumIpDevices(UInt32 nTypeMask, ZP_ENUMDEVICEPROC pEnumProc, IntPtr pUserData,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, UInt32 nFlags = ZP_SF_UPDATE);

#if ZP_LOG
        [DllImport(ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_SetLog")]
        public static extern int ZP_SetLog(string sSvrAddr, string sFileName, UInt32 nMsgMask);

        [DllImport(ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetLog")]
        public static extern int ZP_GetLog(ref string sSvrAddrBuf, int nSABufSize, ref string sFileNameBuf, int nFNBufSize, ref UInt32 nMsgMask);

        [DllImport(ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_AddLog")]
        public static extern int ZP_AddLog(char chSrc, int nMsgType; string sText);
#endif // ZP_LOG
    }
}
