using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ZPort;

namespace ZREADER {
    #region типы
    // Тип считывателя
    public enum ZR_RD_TYPE {
        ZR_RD_UNDEF = 0,
        ZR_RD_Z2U,				// Z-2 USB
        ZR_RD_M3A,				// Matrix III Rd-All
        ZR_RD_Z2M,				// Z-2 USB MF
        ZR_RD_M3N,				// Matrix III Net
        ZR_RD_CPZ2MF,			// CP-Z-2MF
        ZR_RD_Z2EHR,            // Z-2 EHR
        ZR_RD_Z2BASE,           // Z-2 Base
        ZR_RD_M5                // Matrix V
    }
    // Тип карты
    public enum ZR_CARD_TYPE {
        ZR_CD_UNDEF = 0,
        ZR_CD_EM,				// Em-Marine
        ZR_CD_HID,				// Hid
        ZR_CD_IC,				// iCode
        ZR_CD_UL,				// Mifare UltraLight
        ZR_CD_1K,				// Mifare Classic 1K
        ZR_CD_4K,				// Mifare Classic 4K
        ZR_CD_DF,				// Mifare DESFire
        ZR_CD_PX,				// Mifare ProX
        ZR_CD_COD433F,          // Cod433 Fix
        ZR_CD_COD433,           // Cod433
        ZR_CD_DALLAS,           // Dallas
        ZR_CD_CAME433,          // радиобрелок CAME
        ZR_CD_PLUS,				// Mifare Plus
        ZR_CD_PLUS1K,			// Mifare Plus 1K
        ZR_CD_PLUS2K,			// Mifare Plus 2K
        ZR_CD_PLUS4K,			// Mifare Plus 4K
        ZR_CD_MINI				// Mifare Mini
    }
    public enum ZR_IND_STATE {
        ZR_IND_NO_CHANGE = 0,
        ZR_IND_ON,
        ZR_IND_OFF,
        ZR_IND_AUTO
    }
    // PSK CF (T5557)
    public enum ZR_T57_PSK {
        T57_PSK_UNDEF = -1,
        T57_PSK_RF2,
        T57_PSK_RF4,
        T57_PSK_RF8,
        T57_PSK_RES
    }
    #endregion

    #region структуры
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ZR_RD_NOTIFY_SETTINGS {
        public UInt32 nNMask;                           // Маска типов уведомлений (ZR_RNF_..)
        public ZP_NOTIFYPROC pfnCallback;         // Callback-функция
        public IntPtr pUserData;                        // Параметр для Callback-функции
        public UInt32 nCheckCardPeriod;                 // Период сканирования карт в поле считывателя Z2USB MF и считывателя Matrix III Net в мс (Если =0, используется значение по-умолчанию, 300)
        public UInt32 nCheckInputPeriod;                // Период проверки состояния входов для Matrix III Net (Если =0, используется значение по-умолчанию, никогда)

        public ZR_RD_NOTIFY_SETTINGS(UInt32 _nNMask, ZP_NOTIFYPROC _pfnCallback, IntPtr _pUserData = default(IntPtr), UInt32 _nCheckCardPeriod = 0, UInt32 _nCheckInputPeriod = 0) {
            nNMask = _nNMask;
            pfnCallback = _pfnCallback;
            pUserData = _pUserData;
            nCheckCardPeriod = _nCheckCardPeriod;
            nCheckInputPeriod = _nCheckInputPeriod;
        }
    }
    // Информация о считывателе, возвращаемая функцией ZR_EnumReaders
    // Информация о считывателе, возвращаемая функциями: ZR_Rd_Open, ZR_Rd_AttachPort и ZR_Rd_GetInformation
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public class ZR_RD_INFO {
        public ZP_DEVICE_INFO rBase;
        public ZR_RD_TYPE nType;                // Тип считывателя
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pszLinesBuf;              // Буфер для информационных строк
        public int nLinesBufMax;                // Размер буфера в символах, включая завершающий '\0'
    }
    // Параметры открытия считывателя, используемые функциями: ZR_Rd_Open и ZR_UpdateRdFirmware
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZR_RD_OPEN_PARAMS {
        public ZPort.ZP_PORT_TYPE nPortType;    // Тип порта
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pszName;                  // Имя порта. Если =NULL, то используется hPort
        public IntPtr hPort;                    // Дескриптор порта, полученный функцией ZP_Open
        public ZR_RD_TYPE nRdType;              // Тип считывателя. Если =ZR_RD_UNDEF, то автоопределение
        public IntPtr pWait;                    // Параметры ожидания. Может быть =NULL.
        public Byte nStopBits;
        public int nMode;

        public ZR_RD_OPEN_PARAMS(ZPort.ZP_PORT_TYPE _nType, string _sName, IntPtr _hPort = default(IntPtr),
            ZR_RD_TYPE _nRdType = ZR_RD_TYPE.ZR_RD_UNDEF, IntPtr _pWS = default(IntPtr), Byte _nStopBits = 2, int _nMode = 0) {
            nPortType = _nType;
            pszName = _sName;
            hPort = _hPort;
            nRdType = _nRdType;
            pWait = _pWS;
            nStopBits = _nStopBits;
            nMode = _nMode;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZR_CARD_INFO {
        public ZR_CARD_TYPE nType;              // Тип карты
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Byte[] nNum;                     // Номер карты
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZR_T57_CONFIG {
        public bool fXMode;                     // True, если X-Mode, иначе - e5550 compatible
        public UInt32 nMasterKey;
        public UInt32 nDataRate;
        public UInt32 nModulation;
        public ZR_T57_PSK nPSK_CF;
        public bool fAOR;
        public bool fOTP;                       // (только в XMode)
        public int nMaxBlock;
        public bool fPsw;                       // True, если пароль установлен
        public bool fST_Seq_Ter;                // (только в e5550)
        public bool fSST_Seq_StMrk;             // (только в XMode)
        public bool fFastWrite;
        public bool fInverseData;               // (только в XMode)
        public bool fPOR_Delay;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZR_M3N_CONFIG {
        public Byte nWorkMode;
        public Byte nOutZumm;
        public Byte nOutTM;
        public Byte nOutExit;
        public Byte nOutLock;
        public Byte nOutDoor;
        public Byte nProt;
        public Byte nFlags;                     // 0 Impulse, 1 No card, 2 card num
        public Byte nCardFormat;
        public Byte nSecurityMode;
        UInt16 Reserved1;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZR_IND_FLASH {
        public ZR_IND_STATE nRed;
        public ZR_IND_STATE nGreen;
        public ZR_IND_STATE nSound;
        public UInt32 nDuration;                // ms

        public ZR_IND_FLASH(ZR_IND_STATE _nRed, ZR_IND_STATE _nGreen, ZR_IND_STATE _nSound, UInt32 _nDuration) {
            nRed = _nRed;
            nGreen = _nGreen;
            nSound = _nSound;
            nDuration = _nDuration;
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEMTIME {
        public short wYear;
        public short wMonth;
        public short wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZR_ENUMRDSPROC([In] [MarshalAs(UnmanagedType.LPStruct)] ZR_RD_INFO pInfo, ref ZP_PORT_INFO pPort, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZR_PROCESSCALLBACK(int nPos, int nMax, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZR_ENUMCARDSPROC(ref ZR_CARD_INFO pInfo, IntPtr pUserData);
    #endregion

    class ZRIntf {
        #region Константы (Версия)
        public const int ZR_SDK_VER_MAJOR = 3;
        public const int ZR_SDK_VER_MINOR = 21;
        #endregion

        #region Константы (Ошибки)
        public static readonly int S_OK = 0;                                                // Операция выполнена успешно
        public static readonly int E_FAIL = unchecked((int)0x80000008);                     // Другая ошибка
        public static readonly int E_OUTOFMEMORY = unchecked((int)0x80000002);              // Недостаточно памяти для обработки команды
        public static readonly int E_INVALIDARG = unchecked((int)0x80000003);               // Неправильный параметр
        public static readonly int E_NOINTERFACE = unchecked((int)0x80000004);              // Функция не поддерживается

        public static readonly int ZR_E_NOANSWER = unchecked((int)0x80040302);	            // Нет ответа
        public static readonly int ZR_E_BADANSWER = unchecked((int)0x80040303);	            // Нераспознанный ответ
        public static readonly int ZR_E_CARDNOTSELECT = unchecked((int)0x80040304);	        // Карта не выбрана с помощью функции ZR_Rd_SelectCard
        public static readonly int ZR_E_NOCARD = unchecked((int)0x80040305);	            // Карта не обнаружена
        public static readonly int ZR_E_WRONGZPORT = unchecked((int)0x80040306);	        // Не правильная версия ZPort.dll
        public static readonly int ZR_E_RDERROR = unchecked((int)0x80040307);	            // Неизвестная ошибка считывателя
        public static readonly int ZR_E_CARDACCESS = unchecked((int)0x80040308);	        // Нет доступа к карте
        public static readonly int ZR_E_PAGELOCK = unchecked((int)0x80040309);	            // Страница заблокирована
        public static readonly int ZR_E_NOT57 = unchecked((int)0x8004030A);	                // Карта не типа T5557
        public static readonly int ZR_E_NOWRITET57 = unchecked((int)0x8004030B);	        // Не удалось записать на T5557 (блок заблокирован для записи или указан неправильный пароль)
        public static readonly int ZR_E_INVMODEL = unchecked((int)0x8004030C);	            // Несоответствие модели (при прошивке)
        public static readonly int ZR_E_INVBOOTFILE = unchecked((int)0x8004030D);	        // Некорректные данные прошивки
        public static readonly int ZR_E_BUFFEROVERFLOWN = unchecked((int)0x8004030E);	    // Буфер переполнен
        public static readonly int ZR_E_KEYNOTFOUND = unchecked((int)0x8004030F);	        // Подходящий ключ не найден
        public static readonly int ZR_E_AUTHORIZE = unchecked((int)0x80040310);	            // Ошибка авторизации карты
        public static readonly int ZR_E_CARDNACK = unchecked((int)0x80040311);	            // Карта отказала в исполнении команды
        public static readonly int ZR_E_RESEXHAUSTED = unchecked((int)0x80040312);	        // Ресурсы устройства исчерпаны
        public static readonly int ZR_E_PREVNOTCOMLETED = unchecked((int)0x80040313);	    // Предыдущая команда не завершена

        [Obsolete("use ZR_SUCCESS")]
        public static readonly int ZR_SUCCESS = S_OK;
        [Obsolete("use ZPIntf.ZP_S_CANCELLED")]
        public static readonly int ZR_E_CANCELLED = ZPIntf.ZP_S_CANCELLED;
        [Obsolete("use ZPIntf.ZP_S_NOTFOUND")]
        public static readonly int ZR_E_NOT_FOUND = ZPIntf.ZP_S_NOTFOUND;
        [Obsolete("use E_INVALIDARG")]
        public static readonly int ZR_E_INVALID_PARAM = E_INVALIDARG;
        [Obsolete("use ZPIntf.ZP_E_OPENNOTEXIST")]
        public static readonly int ZR_E_OPEN_NOT_EXIST = ZPIntf.ZP_E_OPENNOTEXIST;
        [Obsolete("use ZPIntf.ZP_E_OPENACCESS")]
        public static readonly int ZR_E_OPEN_ACCESS = ZPIntf.ZP_E_OPENACCESS;
        [Obsolete("use ZPIntf.ZP_E_OPENPORT")]
        public static readonly int ZR_E_OPEN_PORT = ZPIntf.ZP_E_OPENPORT;
        [Obsolete("use ZPIntf.ZP_E_PORTIO")]
        public static readonly int ZR_E_PORT_IO_ERROR = ZPIntf.ZP_E_PORTIO;
        [Obsolete("use ZPIntf.ZP_E_PORTSETUP")]
        public static readonly int ZR_E_PORT_SETUP = ZPIntf.ZP_E_PORTSETUP;
        [Obsolete("use ZPIntf.ZP_E_LOADFTD2XX")]
        public static readonly int ZR_E_LOAD_FTD2XX = ZPIntf.ZP_E_LOADFTD2XX;
        [Obsolete("use ZPIntf.ZP_E_SOCKET")]
        public static readonly int ZR_E_INIT_SOCKET = ZPIntf.ZP_E_SOCKET;
        [Obsolete("use ZPIntf.ZP_E_SERVERCLOSE")]
        public static readonly int ZR_E_SERVERCLOSE = ZPIntf.ZP_E_SERVERCLOSE;
        [Obsolete("use E_OUTOFMEMORY")]
        public static readonly int ZR_E_NOT_ENOUGH_MEMORY = E_OUTOFMEMORY;
        [Obsolete("use E_NOINTERFACE")]
        public static readonly int ZR_E_UNSUPPORT = E_NOINTERFACE;
        [Obsolete("use ZPIntf.ZP_E_NOTINITALIZED")]
        public static readonly int ZR_E_NOT_INITALIZED = ZPIntf.ZP_E_NOTINITALIZED;
        [Obsolete("use E_FAIL")]
        public static readonly int ZR_E_CREATE_EVENT = E_FAIL;
        [Obsolete("use ZR_E_INSUFFICIENTBUFFER")]
        public static readonly int ZR_E_INSUFFICIENT_BUFFER = ZPIntf.ZP_E_INSUFFICIENTBUFFER;
        [Obsolete("use ZR_E_NOANSWER")]
        public static readonly int ZR_E_NO_ANSWER = ZR_E_NOANSWER;
        [Obsolete("use ZR_E_BADANSWER")]
        public static readonly int ZR_E_BAD_ANSWER = ZR_E_BADANSWER;
        [Obsolete("use ZR_E_CARDNOTSELECT")]
        public static readonly int ZR_E_CARD_NOT_SELECT = ZR_E_CARDNOTSELECT;
        [Obsolete("use ZR_E_NOCARD")]
        public static readonly int ZR_E_NO_CARD = ZR_E_NOCARD;
        [Obsolete("use ZR_E_WRONGZPORT")]
        public static readonly int ZR_E_WRONG_ZPORT_VERSION = ZR_E_WRONGZPORT;
        [Obsolete("use ZR_E_RDERROR")]
        public static readonly int ZR_E_RD_OTHER = ZR_E_RDERROR;
        [Obsolete("use ZR_E_CARDACCESS")]
        public static readonly int ZR_E_CARD_ACCESS = ZR_E_CARDACCESS;
        [Obsolete("use ZR_E_PAGELOCK")]
        public static readonly int ZR_E_PAGE_LOCK = ZR_E_PAGELOCK;
        [Obsolete("use ZR_E_NOT57")]
        public static readonly int ZR_E_NO_T57 = ZR_E_NOT57;
        [Obsolete("use ZR_E_NOWRITET57")]
        public static readonly int ZR_E_NO_WRITE_T57 = ZR_E_NOWRITET57;
        [Obsolete("use ZR_E_INVMODEL")]
        public static readonly int ZR_E_INV_MODEL = ZR_E_INVMODEL;
        [Obsolete("use ZR_E_INVBOOTFILE")]
        public static readonly int ZR_E_INV_BOOTFILE = ZR_E_INVBOOTFILE;
        [Obsolete("use ZR_E_BUFFEROVERFLOWN")]
        public static readonly int ZR_E_BUFFER_OVERFLOWN = ZR_E_BUFFEROVERFLOWN;
        [Obsolete("use ZR_E_KEYNOTFOUND")]
        public static readonly int ZR_E_KEY_NOT_FOUND = ZR_E_KEYNOTFOUND;
        [Obsolete("use ZR_E_RDERROR")]
        public static readonly int ZR_E_MIF_FCS = ZR_E_RDERROR;
        [Obsolete("use ZR_E_RDERROR")]
        public static readonly int ZR_E_MIF_INV_CMD = ZR_E_RDERROR;
        [Obsolete("use ZR_E_RDERROR")]
        public static readonly int ZR_E_MIF_INV_PAR = ZR_E_RDERROR;
        [Obsolete("use ZG_E_RESEXHAUSTED")]
        public static readonly int ZR_E_MIF_RES = ZR_E_RESEXHAUSTED;
        [Obsolete("use ZR_E_RDERROR")]
        public static readonly int ZR_E_MIF_RD_DOWN = ZR_E_RDERROR;
        [Obsolete("use ZR_E_NOCARD")]
        public static readonly int ZR_E_MIF_NO_CARD = ZR_E_NOCARD;
        [Obsolete("use ZR_E_RDERROR")]
        public static readonly int ZR_E_MIF_CD_ANSWER = ZR_E_RDERROR;
        [Obsolete("use ZR_E_AUTHORIZE")]
        public static readonly int ZR_E_MIF_AUTH = ZR_E_AUTHORIZE;
        [Obsolete("use ZR_E_CARDNACK")]
        public static readonly int ZR_E_MIF_CD_NACK = ZR_E_CARDNACK;
        [Obsolete("use ZG_E_PREVNOTCOMLETED")]
        public static readonly int ZR_E_MIF_PREV_CMD_NC = ZR_E_PREVNOTCOMLETED;
        [Obsolete("use E_FAIL")]
        public static readonly int ZR_E_OTHER = E_FAIL;
        #endregion

        #region Константы
        public const uint ZR_IF_ERROR_LOG = 0x100;          // Записывать лог
        public const uint ZR_DEVTYPE_CVT = 0;
        public const uint ZR_DEVTYPE_Z2U = 3;
        public const uint ZR_DEVTYPE_Z2M = 4;
        #endregion

        #region Константы (Уведомления)
        public const uint ZR_RNF_PLACE_CARD = 1;            // ZR_RN_CARD_INSERT / ZR_RN_CARD_REMOVE
        public const uint ZR_RNF_INPUT_CHANGE = 2;          // ZR_RN_INPUT_CHANGE
        public const uint ZR_RNF_IND_FLASH_END = 4;         // ZR_RN_IND_FLASH_END
        public const uint ZR_RNF_ERROR = 8;                 // ZR_RN_ERROR
        public const uint ZR_RNF_WND_SYNC = 0x4000;         // Синхронизировать с очередью сообщений Windows (не работает если при инициализации установлен флаг ZP_IF_NO_MSG_LOOP)
        public const uint ZR_RNF_ONLY_NOTIFY = 0x8000;      // Только уведомлять о добавлении новых сообщений в очередь

        public const uint ZR_RN_CARD_INSERT = 100;          // Карта поднесена ((PZR_CARD_INFO)lParam)
        public const uint ZR_RN_CARD_REMOVE = 101;          // Карта удалена ((PZR_CARD_INFO)lParam, может быть = NULL)
        public const uint ZR_RN_CARD_UNKNOWN = 102;         // Неизвестное сообщение от считывателя, (LPWCSTR)nMsgParam - текст сообщения
        public const uint ZR_RN_INPUT_CHANGE = 103;         // Изменилось состояние входов Matrix III Net (nMsgParam - новое состояние входов)
        public const uint ZR_RN_IND_FLASH_END = 104;        // Очередь индикации завершена (Z-2 USB MF, CPZ-2-MF, Matrix III Net), без параметра
        public const uint ZR_RN_ERROR = 105;                // Произошла ошибка в ните (HRESULT*)nMsgParam - код ошибки
        #endregion

        #region Константы (T5557)
        // Флаги для функции ZR_Rd_FindT57 \ ZR_Rd_ReadT57Block \ ZR_Rd_WriteT57Block
        public const uint ZR_T57F_INIT = 1;                 // Разрешить инициализировать, если не удалось подобрать параметры (только в ZR_Rd_FindT57)
        public const uint ZR_T57F_PSW = 2;                  // Использовать пароль
        public const uint ZR_T57F_BLOCK = 4;                // Блокировать дальнейшую перезапись блока (только в ZR_Rd_WriteT57Block)
        // Modulation
        public const uint T57_MOD_DIRECT = 0;               // 0 0 0 0 0
        public const uint T57_MOD_PSK1 = 2;                 // 0 0 0 1 0
        public const uint T57_MOD_PSK2 = 4;                 // 0 0 1 0 0
        public const uint T57_MOD_PSK3 = 6;                 // 0 0 1 1 0
        public const uint T57_MOD_FSK1 = 8;                 // 0 1 0 0 0
        public const uint T57_MOD_FSK2 = 0xA;               // 0 1 0 1 0
        public const uint T57_MOD_FSK1A = 0xC;              // 0 1 1 0 0
        public const uint T57_MOD_FSK2A = 0xE;              // 0 1 1 1 0
        public const uint T57_MOD_MANCHESTER = 0x10;        // 1 0 0 0 0
        public const uint T57_MOD_BIPHASE50 = 0x1;          // 0 0 0 0 1
        public const uint T57_MOD_BIPHASE57 = 0x11;         // 1 0 0 0 1
        #endregion

        #region Константы (Индикаторы)
        // Флаги состояния индикатора
        public const uint ZR_ISF_ON = 1;
        public const uint ZR_ISF_AUTO = 2;

        public const uint ZR_MAX_IND_FLASH = 15;
        #endregion

        public const string ZrDllName = "ZReader.dll";

        //Функции библиотеки
        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_GetVersion")]
        public static extern UInt32 ZR_GetVersion();

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Initialize")]
        public static extern int ZR_Initialize(UInt32 nFlags, IntPtr ppObj = default(IntPtr));

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Initialize")]
        public static extern int ZR_Initialize(UInt32 nFlags, out IntPtr ppObj);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Finalyze")]
        public static extern int ZR_Finalyze();

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_CloseHandle")]
        public static extern int ZR_CloseHandle(IntPtr hHandle);

        public static int ZR_EnumSerialPorts(ZP_ENUMPORTSPROC pEnumProc, IntPtr pUserData) {
            return ZPIntf.ZP_EnumSerialPorts(0x19, pEnumProc, pUserData);
        }

        [DllImport(ZPIntf.ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_EnumSerialDevices")]
        public static extern int ZP_EnumSerialDevices(UInt32 nTypeMask,
            [In] [MarshalAs(UnmanagedType.LPArray)] ZP_PORT_ADDR[] pPorts, int nPCount, ZR_ENUMRDSPROC pEnumProc, IntPtr pUserData,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, bool fUpdate = true);
        public static int ZR_EnumReaders([In] [MarshalAs(UnmanagedType.LPArray)] ZP_PORT_ADDR[] pPorts, int nPCount, ZR_ENUMRDSPROC pEnumProc, IntPtr pUserData,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, bool fUpdate = true) {
            return ZP_EnumSerialDevices(0x19, pPorts, nPCount, pEnumProc, pUserData, pWS, fUpdate);
        }

        public static int ZR_FindReader([In] [MarshalAs(UnmanagedType.LPArray)] ZP_PORT_ADDR[] pPorts, int nPCount,
            [MarshalAs(UnmanagedType.LPStruct)] ref ZR_RD_INFO pInfo, ref ZP_PORT_INFO pPort,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, UInt32 fUpdate = ZPIntf.ZP_SF_UPDATE) {
            int nRes, nSize;
            nSize = Marshal.SizeOf(pInfo);
            IntPtr pnt = Marshal.AllocHGlobal(nSize);
            try {
                nRes = ZPIntf.ZP_FindSerialDevice(0x19, pPorts, nPCount, pnt, nSize, ref pPort, pWS, fUpdate);
                pInfo = (ZR_RD_INFO)Marshal.PtrToStructure(pnt, typeof(ZR_RD_INFO));
            } finally {
                Marshal.FreeHGlobal(pnt);
            }
            return nRes;
        }

        public static int ZR_FindNotification(ref IntPtr pHandle, ref ZP_NOTIFY_SETTINGS pSettings) {
            pSettings.nSDevTypes |= 0x19;
            return ZPIntf.ZP_FindNotification(ref pHandle, ref pSettings);
        }
        [Obsolete("use ZR_CloseHandle")]
        public static int ZR_CloseNotification(IntPtr hHandle) {
            return ZR_CloseHandle(hHandle);
        }

        public static int ZR_ProcessMessages(IntPtr hHandle, ZP_NOTIFYPROC pEnumProc, IntPtr pUserData) {
            return ZPIntf.ZP_ProcessMessages(hHandle, pEnumProc, pUserData);
        }

        [DllImport(ZrDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_UpdateRdFirmware")]
        public static extern int ZR_UpdateRdFirmware(ref ZR_RD_OPEN_PARAMS pParams, [In] byte[] pData, int nCount,
            ZR_PROCESSCALLBACK pfnCallback, IntPtr pUserData);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Open")]
        public static extern int ZR_Rd_Open(ref IntPtr pHandle, ref ZR_RD_OPEN_PARAMS pParams,
            [In, Out] [MarshalAs(UnmanagedType.LPStruct)] ZR_RD_INFO pInfo = null);
        [Obsolete("use ZR_CloseHandle")]
        public static int ZR_Rd_Close(IntPtr hHandle) {
            return ZR_CloseHandle(hHandle);
        }

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_DettachPort")]
        public static extern int ZR_Rd_DettachPort(IntPtr hHandle, ref IntPtr pPortHandle);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_GetWaitSettings")]
        public static extern int ZR_Rd_GetWaitSettings(IntPtr hHandle, ref ZP_WAIT_SETTINGS pSetting);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_SetWaitSettings")]
        public static extern int ZR_Rd_SetWaitSettings(IntPtr hHandle, ref ZP_WAIT_SETTINGS pSetting);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_SetCapture")]
        public static extern int ZR_Rd_SetCapture(IntPtr hHandle);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_ReleaseCapture")]
        public static extern int ZR_Rd_ReleaseCapture(IntPtr hHandle);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_GetInformation")]
        public static extern int ZR_Rd_GetInformation(IntPtr hHandle, ref ZR_RD_INFO pInfo);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_UpdateFirmware")]
        public static extern int ZR_Rd_UpdateFirmware(IntPtr hHandle, [In] byte[] pData, int nCount,
            ZR_PROCESSCALLBACK pfnCallback, IntPtr pUserData);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_SetNotification")]
        public static extern int ZR_Rd_SetNotification(IntPtr hHandle,
            [In, Out] [MarshalAs(UnmanagedType.LPStruct)] ZR_RD_NOTIFY_SETTINGS pSettings);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_ProcessMessages")]
        public static extern int ZR_Rd_ProcessMessages(IntPtr hHandle, ZPort.ZP_NOTIFYPROC pEnumProc, IntPtr pUserData);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_FindCard")]
        public static extern int ZR_Rd_FindCard(IntPtr hHandle, ref ZR_CARD_TYPE pType, [In, Out] Byte[] pNum);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_ReadULCard4Page")]
        public static extern int ZR_Rd_ReadULCard4Page(IntPtr hHandle, int nPageN, [In, Out] Byte[] pBuf16);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_WriteULCardPage")]
        public static extern int ZR_Rd_WriteULCardPage(IntPtr hHandle, int nPageN, [In, Out] Byte[] pData4);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_FindT57")]
        public static extern int ZR_Rd_FindT57(IntPtr hHandle, [In, Out] Byte[] pNum, ref int pPar, UInt32 nPsw, UInt32 nFlags);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_ReadT57Block")]
        public static extern int ZR_Rd_ReadT57Block(IntPtr hHandle, int nBlockN, [In, Out] Byte[] pBuf4,
            int pPar, UInt32 nPsw, UInt32 nFlags);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_WriteT57Block")]
        public static extern int ZR_Rd_WriteT57Block(IntPtr hHandle, int nBlockN, [In, Out] Byte[] pData4,
            int pPar, UInt32 nPsw, UInt32 nFlags);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_ResetT57")]
        public static extern int ZR_Rd_ResetT57(IntPtr hHandle);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_DecodeT57Config")]
        public static extern int ZR_DecodeT57Config(ref ZR_T57_CONFIG pConfig, [In, Out] Byte[] pData4);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_EncodeT57Config")]
        public static extern int ZR_EncodeT57Config([In, Out] Byte[] pBuf4, ref ZR_T57_CONFIG pConfig);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_DecodeT57EmMarine")]
        public static extern int ZR_DecodeT57EmMarine(ref int pBitOffs, [In, Out] Byte[] pNum, [In, Out] Byte[] pData, int nCount);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_EncodeT57EmMarine")]
        public static extern int ZR_EncodeT57EmMarine([In, Out] Byte[] pBuf8_, int nBufSize, int nBitOffs, [In, Out] Byte[] pNum);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_DecodeT57Hid")]
        public static extern int ZR_DecodeT57Hid([In, Out] Byte[] pNum, [In, Out] Byte[] pData12, ref int nWiegand);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_EncodeT57Hid")]
        public static extern int ZR_EncodeT57Hid([In, Out] Byte[] pBuf12, [In, Out] Byte[] pNum, int nWiegand);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_GetEncodedCardNumber")]
        public static extern int ZR_Rd_GetEncodedCardNumber(IntPtr hHandle, ref ZR_CARD_TYPE pType, [In, Out] Byte[] pNum,
            [In, Out] Byte[] pBuf, int nBufSize, ref int pRCount);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_EnumCards")]
        public static extern int ZR_Rd_EnumCards(IntPtr hHandle, ZR_ENUMCARDSPROC pEnumProc, IntPtr pUserData);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_SelectCard")]
        public static extern int ZR_Rd_SelectCard(IntPtr hHandle, [In, Out] Byte[] pNum);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_AuthorizeSect")]
        public static extern int ZR_Rd_AuthorizeSect(IntPtr hHandle, int nBlockN, bool fKeyB, UInt64 nKey);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_AuthorizeSectByEKey")]
        public static extern int ZR_Rd_AuthorizeSectByEKey(IntPtr hHandle, int nBlockN, bool fKeyB, UInt32 nKeyMask,
            ref int pRKeyIdx);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_ReadMfCardBlock")]
        public static extern int ZR_Rd_ReadMfCardBlock(IntPtr hHandle, int nBlockN, [In, Out] Byte[] pBuf16);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_WriteMfCardBlock")]
        public static extern int ZR_Rd_WriteMfCardBlock(IntPtr hHandle, int nBlockN, [In, Out] Byte[] pData16);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_WriteMfCardBlock4")]
        public static extern int ZR_Rd_WriteMfCardBlock4(IntPtr hHandle, int nBlockN, [In, Out] Byte[] pData4);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_GetIndicatorState")]
        public static extern int ZR_Rd_GetIndicatorState(IntPtr hHandle, ref UInt32 pRed, ref UInt32 pGreen, ref UInt32 pSound);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_SetIndicatorState")]
        public static extern int ZR_Rd_SetIndicatorState(IntPtr hHandle, UInt32 nRed, UInt32 nGreen, UInt32 nSound);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_AddIndicatorFlash")]
        public static extern int ZR_Rd_AddIndicatorFlash(IntPtr hHandle, [In, Out] ZR_IND_FLASH[] pRecs, int nCount,
            bool fReset, ref int pRCount);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_BreakIndicatorFlash")]
        public static extern int ZR_Rd_BreakIndicatorFlash(IntPtr hHandle, bool fAutoMode);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_GetIndicatorFlashAvailable")]
        public static extern int ZR_Rd_GetIndicatorFlashAvailable(IntPtr hHandle, ref int pCount);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Reset1356")]
        public static extern int ZR_Rd_Reset1356(IntPtr hHandle, UInt16 nWaitMs);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_PowerOff")]
        public static extern int ZR_Rd_PowerOff(IntPtr hHandle);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Request")]
        public static extern int ZR_Rd_Request(IntPtr hHandle, bool fWakeUp, ref ZR_CARD_TYPE pType, ref UInt16 pATQ);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Halt")]
        public static extern int ZR_Rd_Halt(IntPtr hHandle);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_A_S")]
        public static extern int ZR_Rd_A_S(IntPtr hHandle, [In, Out] Byte[] pNum, ref Byte pSAK);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_R_A_S")]
        public static extern int ZR_Rd_R_A_S(IntPtr hHandle, bool fWakeUp, ref ZR_CARD_TYPE pType,
            [In, Out] Byte[] pNum, ref Byte pSAK, ref UInt16 pATQ);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_R_R")]
        public static extern int ZR_Rd_R_R(IntPtr hHandle, [In, Out] Byte[] pNum, bool fWakeUp);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Auth")]
        public static extern int ZR_Rd_Auth(IntPtr hHandle, UInt32 nAddr, bool fKeyB, ref UInt64 pKey,
            UInt32 nEKeyMask, ref int pEKeyIdx);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Read16")]
        public static extern int ZR_Rd_Read16(IntPtr hHandle, UInt32 nAddr, [In, Out] Byte[] pBuf16);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Write16")]
        public static extern int ZR_Rd_Write16(IntPtr hHandle, UInt32 nAddr, [In, Out] Byte[] pData16);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Write4")]
        public static extern int ZR_Rd_Write4(IntPtr hHandle, UInt32 nAddr, [In, Out] Byte[] pData4);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Increment")]
        public static extern int ZR_Rd_Increment(IntPtr hHandle, UInt32 nAddr, UInt32 nValue);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Decrement")]
        public static extern int ZR_Rd_Decrement(IntPtr hHandle, UInt32 nAddr, UInt32 nValue);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Transfer")]
        public static extern int ZR_Rd_Transfer(IntPtr hHandle, UInt32 nAddr);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_Restore")]
        public static extern int ZR_Rd_Restore(IntPtr hHandle, UInt32 nAddr);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Rd_WriteKeyToEEPROM")]
        public static extern int ZR_Rd_WriteKeyToEEPROM(IntPtr hHandle, UInt32 nAddr, bool fKeyB, ref UInt64 pKey);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_EncodeMfAccessBits")]
        public static extern int ZR_EncodeMfAccessBits(int nAreaN, [In, Out] Byte[] pBuf3, UInt32 nBits);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_DecodeMfAccessBits")]
        public static extern int ZR_DecodeMfAccessBits(int nAreaN, ref UInt32 pBits, [In, Out] Byte[] pData3);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_M3n_ActivatePowerKey")]
        public static extern int ZR_M3n_ActivatePowerKey(IntPtr hHandle, bool fForce, UInt32 nTimeMs);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_M3n_SetOutputs")]
        public static extern int ZR_M3n_SetOutputs(IntPtr hHandle, UInt32 nMask, UInt32 nOutBits);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_M3n_GetInputs")]
        public static extern int ZR_M3n_GetInputs(IntPtr hHandle, ref UInt32 pFlags);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_M3n_SetConfig")]
        public static extern int ZR_M3n_SetConfig(IntPtr hHandle, ref ZR_M3N_CONFIG pConfig);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_M3n_GetConfig")]
        public static extern int ZR_M3n_GetConfig(IntPtr hHandle, ref ZR_M3N_CONFIG pConfig);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_M3n_SetSecurity")]
        public static extern int ZR_M3n_SetSecurity(IntPtr hHandle, int nBlockN, UInt32 nKeyMask, bool fKeyB);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_M3n_GetSecurity")]
        public static extern int ZR_M3n_GetSecurity(IntPtr hHandle, ref int nBlockN, ref UInt32 nKeyMask, ref bool fKeyB);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_M3n_SetClock")]
        public static extern int ZR_M3n_SetClock(IntPtr hHandle, ref SYSTEMTIME pTime);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_M3n_GetClock")]
        public static extern int ZR_M3n_GetClock(IntPtr hHandle, ref SYSTEMTIME pTime);

        [DllImport(ZrDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Z2b_SetFormat")]
        public static extern int ZR_Z2b_SetFormat(IntPtr hHandle,
            string pFmt, string pszArg, string pszNoCard, bool fSaveEE);

        [DllImport(ZrDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Z2b_GetFormat")]
        public static extern int ZR_Z2b_GetFormat(IntPtr hHandle,
            ref string pFmtBuf, int nFmtBufSize, ref string pArgBuf, int nArgBufSize, ref string pNoCardBuf, int nNoCardBufSize);

        [DllImport(ZrDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZR_Z2b_SetPowerState")]
        public static extern int ZR_Z2b_SetPowerState(IntPtr hHandle, bool fOn);

        // Utils
        public static string CardNumToStr(Byte[] aNum, ZR_CARD_TYPE nType) {
            string s;
            switch (nType) {
                case ZR_CARD_TYPE.ZR_CD_EM:
                    s = string.Format("{0},{1:D5}", aNum[3], aNum[1] + (aNum[2] << 8));
                    break;
                case ZR_CARD_TYPE.ZR_CD_HID: {
                        int nFCnt = Math.Min(aNum[0] - 2, 4);
                        int nFacility = 0;
                        int nOffs = 0;
                        for (int i = 0; i < nFCnt; i++, nOffs += 8)
                            nFacility = (aNum[3 + i] << nOffs);
                        s = string.Format("[{0}] {1:D5}", nFacility.ToString(string.Format("X{0}", nFCnt * 2)), aNum[1] + (aNum[2] << 8));
                    }
                    break;
                default: {
                        s = "";
                        for (int i = aNum[0]; i > 0; i--)
                            s = s + aNum[i].ToString("X2");
                    }
                    break;
            }
            return s;
        }
    }
}
