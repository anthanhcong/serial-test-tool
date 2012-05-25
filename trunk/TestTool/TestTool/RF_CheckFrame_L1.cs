using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApplication1
{
    partial class Test_Form
    {
        enum PKB_RX_FSM_STATE
        {
            PKB_RX_IDLE = 0,
            PKB_RX_PREAMBLE_FF,
            PKB_RX_START_OF_PACKET,
            PKB_RX_RECEIVING,
            PKB_RX_CRC
        };

        #region Constant define
            const int PK_MAX_BUFFER_LEN = (0x50); //TODO: eventualmente da spostare: 80 byte complessivi
            const int  PKB_PREAMBLE_FF = (0x0FF);
        #endregion

        static PKB_RX_FSM_STATE PKB_rxState = PKB_RX_FSM_STATE.PKB_RX_IDLE;
        static int byteCount = 0;
        static int packetLength = 0;
        static int packetAndCrcLength = 0;
        static int crcAccumulator = 0;


        
        static byte[] PKB_rxBuffer = new byte [200]; // PK_START_CHAR_BYTES+PK_MAX_BUFFER_LEN+PK_CRC_BYTES
        /*******************************************************************************
        * Name           : PKB_rxCharEvent
        * -----------------------------------------------------------------------------
        * Input          : dd - received character
        * Output         : fills 'Rxwrite' with 'length' to 'Data' frame's fields
        * Returned Value : none
        * Description    : Callback function called by L1 at any character receiving
        *******************************************************************************/
        void PKB_rxCharEvent ( byte p_rxChar )
        {

	        switch( PKB_rxState )
	        {
                case PKB_RX_FSM_STATE.PKB_RX_IDLE:
                    byteCount = 0;
                    packetLength = 0;
                    packetAndCrcLength = 0;
        		
			        if( p_rxChar == PKB_PREAMBLE_FF )
			        {
				        PKB_rxState = PKB_RX_FSM_STATE.PKB_RX_PREAMBLE_FF;
			        }
        		
		        break;

                case PKB_RX_FSM_STATE.PKB_RX_PREAMBLE_FF:
                    if( p_rxChar == SS_SOP_RP30 )
			        {
                        crcAccumulator = 0x5a5a;
                        PKB_rxState = PKB_RX_FSM_STATE.PKB_RX_START_OF_PACKET;
				        PKB_rxBuffer[byteCount] = p_rxChar;
				        byteCount++;
                    }
			        else
			        {
                        PKB_rxState = PKB_RX_FSM_STATE.PKB_RX_IDLE;
                    }
        				
		        break;

                case PKB_RX_FSM_STATE.PKB_RX_START_OF_PACKET:
        		    ///@FIXME (Kien ##): Start Timer for frame
			        // CTRL_timerStart( PKB_RX_TIMER );         
			        PKB_rxState = PKB_RX_FSM_STATE.PKB_RX_RECEIVING;
			        PKB_rxBuffer[byteCount] = p_rxChar;
			        byteCount++;			

			        // The packet len is calculated from the len field in air + 1
			        packetLength = ((0x7F)&(p_rxChar)) + 1;  //TODO: usare la maschera per prendere la len

			        // We have to add 2 bytes to the packet len
			        packetAndCrcLength = packetLength + 2;

                    ///@FIXME (Kien ##): Start Timer for frame
                    /*
                    if( (packetAndCrcLength > PK_MAX_BUFFER_LEN) || (packetAndCrcLength < PK_MIN_LEN) )
                    {
                        PKB_rxState = PKB_RX_IDLE;
                        CTRL_timerStop( PKB_RX_TIMER );
                    }
                    else
                    {
                        PKB_rxState = PKB_RX_RECEIVING;
                    }
                     */
        				
		        break;

                case PKB_RX_FSM_STATE.PKB_RX_RECEIVING:	
			        PKB_rxBuffer[byteCount] = p_rxChar;
			        byteCount++;
			        if( byteCount == packetLength )
			        {
				        PKB_rxState = PKB_RX_FSM_STATE.PKB_RX_CRC;
			        }
		            break;

                case PKB_RX_FSM_STATE.PKB_RX_CRC:
        		
			        // CRC_ACCUM(crcAccumulator,p_rxChar);
                    crcAccumulator = Get_CRC(PKB_rxBuffer, byteCount);
			        byteCount++;
                    

        		
			        if( byteCount == packetAndCrcLength )
			        {
				        PKB_rxState = PKB_RX_FSM_STATE.PKB_RX_IDLE;
				        //HAL_rxPoll(FALSE_SS);
				        byteCount = 0;
				        packetLength = 0;
				        packetAndCrcLength = 0;
                        L2_rxCheckPacket( PKB_rxBuffer,crcAccumulator );        //@NOTE Kien(2.0): SS2.0 call L2 check frame
                        ///@FIXME (Kien ##): Start Timer for frame
                        // CTRL_timerStop( PKB_RX_TIMER );
			        }
        		
		            break;
		        default:
                    PKB_rxState = PKB_RX_FSM_STATE.PKB_RX_IDLE;
		            break;
        	
	        }
        		
        }
    }
}
