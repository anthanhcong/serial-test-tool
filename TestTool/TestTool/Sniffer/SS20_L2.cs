using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    partial class Test_Form
    {
        #region Constant
            const int SS_SOP_RP30 = (0x96);
            const int SS_SOP_RP20 = (0xB2);
            const int DIRECTIONS_NUMBER = 2;
            const int PK_SOP_FIELD_POSITION = 0;
            const int SS_PROTOCOL_UNDEF		= 0; 		/*!< Star Stack Radio Protocol still undefined */
            const int SS_RADIO_PROTOCOL_V20	= 1; 		/*!< Star Stack Radio Protocol Version 2.0 */
            const int SS_RADIO_PROTOCOL_V30 = 2;	/*!< Star Stack Radio Protocol Version 3.0 */
        #endregion

        #region Enum
        enum L2_RX_PACKET_CHECK
        {
	        PACKET_CORRECT = 0,
	        GUN_NOT_YET_BOUND,
	        WRONG_CRC,
	        WRONG_PROTOCOL_HEADER,
	        PACKET_TOO_LONG,
	        PACKET_TOO_SHORT,
	        WRONG_CLIENT_SOURCE_ADDRESS,
	        WRONG_CLIENT_DESTINATION_ADDRESS,
	        WRONG_SERVER_DESTINATION_ADDRESS,
            WRONG_DESTINATION_ADDRESS,
            TX_FSM_IS_NOT_WAITING_ANY_ACK,
            TX_FSM_WRONG_ACK_EXPECTED,
            DUPLICATE_SYNC_WITHOUT_PTS_PACKET,
            DUPLICATED_SYNC_WITH_PTS_PACKET,    
            TX_FSM_WRONG_ACK_SYNC_EXPECTED,
            DUPLICATED_SYNC_CFG_WITHOUT_PTS_PACKET,
            DUPLICATED_SYNC_CFG_WITH_PTS_PACKET,
            TX_FSM_WRONG_SYNC_CFG_ACK_EXPECTED,
            SERVER_DOES_NOT_ACCEPT_BEACONS,
            DUPLICATE_DATA_WITHOUT_PTS_PACKET,
            DUPLICATE_DATA_WITH_PTS_PACKET,
            DUPLICATED_BEACON,
            FAILED_ACK_CALLBACK,
            FAILED_NACK_CALLBACK,
            FAILED_LOCK_CALLBACK,
            TX_FSM_WRONG_BEACON_COUNTER,
            GUN_WAITING_INQUIRY_BEACON,
            WRONG_PACKET_TYPE_IN_INQUIRY_STATE,
            WRONG_PACKET_TYPE_IN_OUT_OF_FIELD_STATE,
            BOUND_CRADLE_WRONG_DOMAIN,
            WRONG_SOURCE_ADDRESS,
            WRONG_DOMAIN_ID,
            NETWORK_NOT_ENABLED,
            WRONG_BEACON_DID,
            PACKET_FROM_BOUND_CRADLE,
	        NO_BEACON_IN_CRADLE,
	        WRONG_CLIENT_SEQUENCE_COUNTER,
	        WRONG_SERVER_SEQUENCE_COUNTER,
	        SS_RP20_NOT_YET_IMPLEMENTED,
            WRONG_DESCRIPTOR,
            WRONG_PACKET_TYPE,
            WRONG_ACK_LENGTH,
            WRONG_PAYLOAD_LENGTH,
            WRONG_ADDRESS,
	        DEVICE_UNDER_RELEASE,
            WRONG_BASESTATION_STATE,
            WRONG_REMOTE_COUNTER,
            WRONG_SEQUENCE_COUNTER,
            SSRP20_NOT_YET_SUPPORTED,
            GENERIC_ERROR
        };
        enum L2_PACKET_ADDITIONAL_INFO
        {
	        L2_PACKET_STANDARD = 0,
	        L2_PACKET_WITH_PTS,
	        L2_PACKET_WITH_DID
        };

        enum L2_ADDRESS_TYPE
        {
	        L2_COMPRESSED_ADDRESS = 0, 	/*!< Compressed Address: 1 byte */
	        L2_STANDARD_ADDRESS = 2,	/*!< Standard Address: 3 bytes */
	        L2_EXTENDED_ADDRESS = 3		/*!< Extended Address: 4 bytes */

        };

        enum L2_PACKET_TYPE
        {
	        L2_P_NULL = 0,
            L2_P_DATA,
            L2_P_SYNC,
            L2P_BECON,
            UNKNOW
        } ;

        #endregion
        /******************************************************************************
        *         : L2_rxCheckPacket
        ****************************************************************************/
        /*!
        * @param [in]   : CRC of the packet
        * @param [out]  : none
        * @return    : L2_RX_PACKET_CHECK
        * @brief     : This function checks the packet correctness in 2 steps:\n
        *                : 1) Packet self-correctness\n
        *                : 2) Packet vs. Configuration correctness\n
        *                : 3) Descriptor loading
        *******************************************************************************/
        L2_RX_PACKET_CHECK L2_rxCheckPacket( byte[] p_L2_rxPacket, int p_CRC )
        {
            L2_RX_PACKET_CHECK packetCheck = L2_RX_PACKET_CHECK.FAILED_ACK_CALLBACK;
            int protocolSOP;
            int packetLength; //indicates the total length of the packet ( HEADER + PAYLOAD )
            int networkDomain;

            // Address Information
            L2_ADDRESS_TYPE dstAddressType;
            L2_ADDRESS_TYPE srcAddressType;
            Int32 dstAddress;
            Int32 srcAddress;

            // Control informaiton
            int control;
            L2_PACKET_TYPE packetType;

            // Transport Information
            Int32 transport;
            int SAP;
            int service;
            
            int sequenceCounter;
            bool remoteCounter;
            bool transportSyncPresent;
            int beaconCounter;
            int globalStatus;
            int sessionKey;
            int PackTimeSt;
            int maxBufferSize;
            int maxBackoff;
            int index;
            int fil_len;
            int value;

            
            protocolSOP = p_L2_rxPacket[0];
            packetLength = p_L2_rxPacket[1];
            networkDomain = p_L2_rxPacket[2];
            // Get Source Address
            index = 3;
            if ((byte)(p_L2_rxPacket[index] & (byte)(0x80)) == 0)
            {
                fil_len = 1;
                srcAddressType = L2_ADDRESS_TYPE.L2_COMPRESSED_ADDRESS;
                srcAddress = p_L2_rxPacket[index];
            }
            else if ((byte)(p_L2_rxPacket[index] & (byte)(0x40)) == 0)
            {
                fil_len = 3;
                srcAddressType = L2_ADDRESS_TYPE.L2_EXTENDED_ADDRESS;
                srcAddress = (p_L2_rxPacket[index] << 16);
                srcAddress += (p_L2_rxPacket[index + 1] << 8);
                srcAddress += (p_L2_rxPacket[index + 2]);
            }
            else
            {
                fil_len = 4;
                srcAddressType = L2_ADDRESS_TYPE.L2_STANDARD_ADDRESS;
                srcAddress = (p_L2_rxPacket[index] << 24);
                srcAddress += (p_L2_rxPacket[index + 1] << 16);
                srcAddress += (p_L2_rxPacket[index + 2] << 8);
                srcAddress += p_L2_rxPacket[index + 3];
            }

            // Get Destination Address
            index += fil_len;
            if ((byte)(p_L2_rxPacket[index] & (byte)(0x80)) == 0)
            {
                fil_len = 1;
                dstAddressType = L2_ADDRESS_TYPE.L2_COMPRESSED_ADDRESS;
                dstAddress = p_L2_rxPacket[index];
            }
            else if ((byte)(p_L2_rxPacket[index] & (byte)(0x40)) == 0)
            {
                fil_len = 3;
                dstAddressType = L2_ADDRESS_TYPE.L2_EXTENDED_ADDRESS;
                dstAddress = (p_L2_rxPacket[index] << 16);
                dstAddress += (p_L2_rxPacket[index + 1] << 8);
                dstAddress += (p_L2_rxPacket[index + 2]);
            }
            else
            {
                fil_len = 4;
                dstAddressType = L2_ADDRESS_TYPE.L2_STANDARD_ADDRESS;
                dstAddress = (p_L2_rxPacket[index] << 24);
                dstAddress += (p_L2_rxPacket[index + 1] << 16);
                dstAddress += (p_L2_rxPacket[index + 2] << 8);
                dstAddress += p_L2_rxPacket[index + 3];
            }

            index += fil_len;
            // Get 1 byte control
            control = p_L2_rxPacket[index];
            value = (control & 0xC0) >> 6;
            switch (value)
            {
                case 0:
                    packetType = L2_PACKET_TYPE.L2_P_NULL;
                    break;
                case 1:
                    packetType = L2_PACKET_TYPE.L2_P_DATA;
                    break;
                case 2:
                    packetType = L2_PACKET_TYPE.L2_P_SYNC;
                    break;
                case 3:
                    packetType = L2_PACKET_TYPE.L2P_BECON;
                    break;
                default:
                    packetType = L2_PACKET_TYPE.UNKNOW;
                    break;
            }
            fil_len = 1;
            index += fil_len;
             
            // Get & analyze transport 3 bytes
            transport = (p_L2_rxPacket[index] << 16) + (p_L2_rxPacket[index+1] << 8) + p_L2_rxPacket[index+2];
            value = p_L2_rxPacket[index];
            SAP = (value & 0x38) >> 3;
            service = value & 0x3;
            sequenceCounter = ((p_L2_rxPacket[index + 1] & 0xFF) << 8) + p_L2_rxPacket[index + 2];

            fil_len = 3;
            index += fil_len;

            // Get Data

	        return(packetCheck);
        }
    }
}
