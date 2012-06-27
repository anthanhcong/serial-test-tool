using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    partial class Test_Form
    {
        enum SS_TERMINAL_ARG
        {
            COMP_ADDR = 0,
            ADDR,
            PROTVER,
            PID,
            RFVER,
            CONNSTATUS
        };

        class SS_TERMINAL{
	        int	    compressedAddr;				/* Source/Gun compressed address, it's the index of the descr_table*/
	        int     addr;						/* Real 32 bit terminal address*/
            int		protVer;					/* Select protocol compliance */
	        int		PID;						/* Product ID */
	        int		RF;							/* RF Stack version - terminal Information */
	        int		connStatus;					/* Terminal Sync status : resync waiting or done
														        *... RX_RESYNC TX_RESYNC */
            //uint16_ss				userTimeout;				/* User defined - transaction timeout - coded in slot number */
            //CTRL_DESC_TIMER			TransactionTout;			/* User defined - transaction timeout - coded in slot number */
            ///* layer specific descriptors */
            //SS_L4_TERMINAL_DESCR *	l4_desc;					/* L4 descriptor pointer*/
            //SS_L3_TERMINAL_DESCR *	l3_desc;					/* L3 descriptor pointer*/
            //SS_L2_TERMINAL_DESCR *	l2_desc;					/* L2 descriptor pointer*/
            ///*round robin*/
            //RR_ROUNDABLE_TX			roundRobin_TX[DIRECTIONS_NUMBER];

            public void Init()
            {
                compressedAddr = new int();
                addr = new int();
                protVer = new int();
                PID = new int();
                connStatus = new int();
                RF = new int();
            }

            public bool SetValue(int var, SS_TERMINAL_ARG arg)
            {
                bool retvar = true;
                switch (arg)
                {
                    case SS_TERMINAL_ARG.COMP_ADDR:
                        compressedAddr = var;
                        break;
                    case SS_TERMINAL_ARG.ADDR:
                        addr = var;
                        break;
                    case SS_TERMINAL_ARG.CONNSTATUS:
                        connStatus = var;
                        break;
                    case SS_TERMINAL_ARG.PID:
                        PID = var;
                        break;
                    case SS_TERMINAL_ARG.PROTVER:
                        protVer = var;
                        break;
                    case SS_TERMINAL_ARG.RFVER:
                        RF = var;
                        break;
                    default:
                        break;
                }

                return retvar;
            }

            public int GetValue(SS_TERMINAL_ARG arg)
            {
                switch (arg)
                {
                    case SS_TERMINAL_ARG.COMP_ADDR:
                        return compressedAddr;
                    case SS_TERMINAL_ARG.ADDR:
                        return addr;
                    case SS_TERMINAL_ARG.CONNSTATUS:
                        return connStatus;
                    case SS_TERMINAL_ARG.PID:
                        return PID;
                    case SS_TERMINAL_ARG.PROTVER:
                        return protVer;
                    case SS_TERMINAL_ARG.RFVER:
                        return RF;
                    default:
                        break;
                }
                return 0;
            }
        };
    }
}
