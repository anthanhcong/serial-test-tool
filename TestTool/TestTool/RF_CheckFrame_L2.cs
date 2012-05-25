using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            L2_P_ACK,								// Ack packet
            L2_P_NACK,								// Nack packet
            //L2_P_UNLOCK = L2_P_NACK,
            L2_P_LOCK,								// Lock packet

	        L2_P_DATA_WITHOUT_PTS,					// DATA
	        L2_P_DATA_WITH_PTS,
            L2_P_DATA_ACK,                          // NOT IMPLEMENTED
            L2_P_DATA_NACK,                         // NOT IMPLEMENTED
            //L2_P_DATA_UNLOCK = L2_P_DATA_NACK,	// NOT IMPLEMENTED
            L2_P_DATA_LOCK,                     	// NOT IMPLEMENTED

            L2_P_SYNC_WITHOUT_PTS,                  //  LL SYNC
	        L2_P_SYNC_WITH_PTS,                     //  LL SYNC
            L2_P_SYNC_ACK,                          //  LL SYNC
	        L2_P_SYNC_CLR_WITHOUT_PTS,              //  TP SYNC
	        L2_P_SYNC_CLR_WITH_PTS,                 //  TP SYNC
            L2_P_SYNC_CLR_ACK,                      //  TP SYNC
            L2_P_SYNC_CFG_WITHOUT_PTS,              //  TP CONFIG
            L2_P_SYNC_CFG_WITH_PTS,                 //  TP CONFIG
            L2_P_SYNC_CFG_ACK,                      //  TP CONFIG

	        L2_P_BEACON_PRESENCE,
            L2_P_BEACON_INQUIRY,
            L2_P_BEACON_DATA,

            L2_P_UNKNOWN_PACKET,
	        L2_NUM_PACKET_TYPE
        } ;

        #endregion

        /******************************************************************************
        *         : L2_rxCheckPacket
        ****************************************************************************//*!
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
	        L2_RX_PACKET_CHECK packetCheck;

	        SS_TERMINAL remoteDescriptor  = new SS_TERMINAL();
            remoteDescriptor.Init();

        /* -----------------------------------------------------------------------------
        *         Packet self-correctness check section
        * ----------------------------------------------------------------------------*/

	        if( p_CRC != 0 )
	        {
		        // CRC error: do not ACK this packet
		        return (L2_RX_PACKET_CHECK.WRONG_CRC);
	        }

	        // CRC is ok. We can choose the appropriate protocol parser
	        // and apply it to the packet.

            packetCheck = L2_setProtocol(p_L2_rxPacket, ref remoteDescriptor);        //@NOTE Kien(2.0): SS_20 check is radio 1.0 or 2.0; Currently 2.0 not support 1.0

            if (packetCheck != L2_RX_PACKET_CHECK.PACKET_CORRECT)
	        {
		        // wrong header: unknown protocol
		        return(packetCheck);
	        }

            // Packet Parsing
            packetCheck = L2_rxParser_SSRP30(p_L2_rxPacket);
            if (packetCheck != L2_RX_PACKET_CHECK.PACKET_CORRECT)
	        {
		        // wrong packet
		        return(packetCheck);
	        }

            // Length check
	        packetCheck = (*L2_rxCheckPacketLength)();

            if (packetCheck != L2_RX_PACKET_CHECK.PACKET_CORRECT)
	        {
		        // wrong length
		        return(packetCheck);
	        }

            // Address check
	        packetCheck = (*L2_rxCheckAddress)();

            if (packetCheck != L2_RX_PACKET_CHECK.PACKET_CORRECT)
	        {
		        // wrong address
		        return(packetCheck);
	        }

        /* -----------------------------------------------------------------------------
        *         Packet vs. Configuration correctness check section
        * ----------------------------------------------------------------------------*/

            packetCheck = (*L2_rxGetRemoteDesc)(&remoteDescriptor);

            if (packetCheck == L2_RX_PACKET_CHECK.PACKET_CORRECT)
	        {
	            //TODO: check synchronization status for current remote descriptor

		        // The packet is correct and is for us: process it specifically and send ACK if needed
		        //packetCheck = PAR_rxProcess[L2_getRxLastPacketType()](remoteDescriptor);

                if(L2_parsingStruct.packetType < L2_NUM_PACKET_TYPE)
                {
			        /* Process incoming packet */
                    packetCheck = PAR_rxProcess[L2_parsingStruct.packetType](remoteDescriptor);
                }
	        }

	        return(packetCheck);

        }

        /*-----------------------------------------------------------------------------*/
        /*!
        * @param [in]   : pointer to the received packet buffer
        * @param [out]  : none
        * @return    : L2_RX_PACKET_CHECK
        * @brief     : This function sets the correct parser for a certain protocol,
        *			: analyzing the SOP char.\n
        *******************************************************************************/
        L2_RX_PACKET_CHECK L2_setProtocol(byte[] p_L2_rxBuffer, ref SS_TERMINAL remoteDescriptor)
        {
            L2_RX_PACKET_CHECK packetCheck;

            switch( p_L2_rxBuffer[PK_SOP_FIELD_POSITION] )
	        {
		        case SS_SOP_RP20:
			        // old protocol stack
                    remoteDescriptor.SetValue(SS_SOP_RP20, SS_TERMINAL_ARG.PROTVER);
                    packetCheck = L2_RX_PACKET_CHECK.SSRP20_NOT_YET_SUPPORTED;
			        //return(SSRP20_NOT_YET_SUPPORTED);
		        break;

		        case SS_SOP_RP30:
			        // new protocol stack
			        // L2_setupStack_SSRP30();
                    remoteDescriptor.SetValue(SS_SOP_RP30, SS_TERMINAL_ARG.PROTVER);
                    packetCheck = L2_RX_PACKET_CHECK.PACKET_CORRECT;
			        //return(PACKET_CORRECT);
		        break;

		        default:
			        // unknown protocol
                    packetCheck = L2_RX_PACKET_CHECK.WRONG_PROTOCOL_HEADER;
			        //return (WRONG_PROTOCOL_HEADER);
		        break;
	        }
            return packetCheck;
        }
        /*******************************************************************************
        * Name           : L2_rxParser_SSRP30
        * -----------------------------------------------------------------------------
        * Input          : a pointer to the packet buffer
        * Output         : none
        * Returned Value : a structure of type L2_RX_PACKET_CHECK
        * Description    : This functions parses the input packet and fills the field in 
        *                : the parser structure
        * Notes          : Star Stack Radio Protocol Version 3.0
        *******************************************************************************/

        L2_RX_PACKET_CHECK L2_rxParser_SSRP30( byte[]p_rxBuffer )
        {	
            
	        //int packetLength;
            bool ptsOrDomainPresence = false;

            L2_PACKET_ADDITIONAL_INFO packetInfo = L2_PACKET_ADDITIONAL_INFO.L2_PACKET_STANDARD;
        	    
            int dstAddressPosition = 0;
	        int srcAddressPosition = 0;
            int controlPosition = 0;
            int frameSequence = 0;
            int tpServiceInformationsPosition = 0;
            int tpFramesIntegrityPosition = 0;
	        int syncConfigPosition = 0;
	        int syncConfigLength = 0;
            int counterPosition = 0;
            int globalStatusPosition = 0;
            int cmdPosition = 0;
            int paramPosition = 0;
            int dataPosition = 0;
            int i = 0;


        /* -----------------------------------------------------------------------------
        *         S T A R T   O F   F R A M E
        * ----------------------------------------------------------------------------*/

	        L2_parsingStruct.protocolSOP = PK_GET_SOP( p_rxBuffer );


        /* -----------------------------------------------------------------------------
        *         L E N G T H
        * ----------------------------------------------------------------------------*/
            
	        // Here we extract the total packet length ( Header + Payload )
	        // After the calculation of payload size, we put in the Header struct only 
	        // the lenght of the data field	
	        // packetLength = PK_GET_LENGTH( p_rxBuffer );
            L2_parsingStruct.packetLength = PK_GET_LENGTH( p_rxBuffer );
            // check of the packet len; these values were checked before,
            // these assert are only for check purposes
            SS_ASSERT(L2_parsingStruct.packetLength >= SS_MIN_PACKET_LENGTH_SSRP30);
            SS_ASSERT(L2_parsingStruct.packetLength <= SS_MAX_PACKET_LENGTH_SSRP30);

        /* -----------------------------------------------------------------------------
        *         D O M A I N   A N D   P T S
        * ----------------------------------------------------------------------------*/

            // check if domain or packet time stamp field is present 	
            ptsOrDomainPresence = PK_GET_DOMAIN_OR_PTS_PRESENCE( p_rxBuffer );
            
            if( ptsOrDomainPresence == PK_PTS_OR_DOMAIN_PRESENT )
            {
                dstAddressPosition = PK_PTS_OR_DOMAIN_FIELD_POSITION + 1;
                // domain or packet time stamp is present
                if(PK_GET_DOMAIN_PRESENCE( p_rxBuffer ) == PK_DOMAIN_PRESENT )
                {
                    // domain present
			        packetInfo = L2_PACKET_WITH_DID;
                    L2_parsingStruct.networkDomain = PK_GET_DOMAIN( p_rxBuffer );
                }
                else
                {
                    // pts present
			        packetInfo = L2_PACKET_WITH_PTS;
			        //TODO: FARE LA CONVERSIONE DA 8 a 16 bit
			        //L2_parsingStruct.headerStruct.PTS = CONV_TO_DESC_TIME(PK_GET_PTS(p_rxBuffer));
			        L2_parsingStruct.PackTimeSt = CONV_TO_DESC_TIME(PK_GET_PTS(p_rxBuffer));
			        L2_parsingStruct.networkDomain = PK_NO_NETWORK_DOMAIN;
                }        
            }
            else
            {
                // no pts or domain present
		        packetInfo = L2_PACKET_STANDARD;
                dstAddressPosition = PK_LENGTH_FIELD_POSITION + 1;
            }

        /* -----------------------------------------------------------------------------
        *         D E S T I N A T I O N    A D D R E S S
        * ----------------------------------------------------------------------------*/
                
            L2_parsingStruct.dstAddressType = L2_rxGetAddressType( p_rxBuffer, dstAddressPosition );
            
            switch( L2_parsingStruct.dstAddressType )
            {
                case L2_COMPRESSED_ADDRESS:
                    
                    L2_parsingStruct.dstAddress = PK_GET_COMPRESSED_ADDRESS(p_rxBuffer,dstAddressPosition);
                    srcAddressPosition = dstAddressPosition + L2_COMPRESSED_ADDRESS_SIZE; 			
                               
                break;
                
                case L2_STANDARD_ADDRESS:
                    
                    L2_parsingStruct.dstAddress = PK_GET_STANDARD_ADDRESS(p_rxBuffer,dstAddressPosition);
			        srcAddressPosition = dstAddressPosition + L2_STANDARD_ADDRESS_SIZE;
                    
                break;
                
                case L2_EXTENDED_ADDRESS:
                    
                    L2_parsingStruct.dstAddress = PK_GET_EXTENDED_ADDRESS(p_rxBuffer,dstAddressPosition);
			        srcAddressPosition = dstAddressPosition + L2_EXTENDED_ADDRESS_SIZE;
                    
                break;    
                
            }

        /* -----------------------------------------------------------------------------
        *         S O U R C E    A D D R E S S
        * ----------------------------------------------------------------------------*/
                
            L2_parsingStruct.srcAddressType = L2_rxGetAddressType( p_rxBuffer, srcAddressPosition );
            
            switch( L2_parsingStruct.srcAddressType )
            {
                case L2_COMPRESSED_ADDRESS:
                    
                    L2_parsingStruct.srcAddress = PK_GET_COMPRESSED_ADDRESS(p_rxBuffer,srcAddressPosition);
			        controlPosition = srcAddressPosition + L2_COMPRESSED_ADDRESS_SIZE;
                               
                break;
                
                case L2_STANDARD_ADDRESS:
                    
                    L2_parsingStruct.srcAddress = PK_GET_STANDARD_ADDRESS(p_rxBuffer,srcAddressPosition);
			        controlPosition = srcAddressPosition + L2_STANDARD_ADDRESS_SIZE;
                    
                break;
                
                case L2_EXTENDED_ADDRESS:
                    
                    L2_parsingStruct.srcAddress = PK_GET_EXTENDED_ADDRESS(p_rxBuffer,srcAddressPosition);
			        controlPosition = srcAddressPosition + L2_EXTENDED_ADDRESS_SIZE;
                    
                break;    
                
            }

        /* -----------------------------------------------------------------------------
        *         C O N T R O L   A N D   T P   Processing
        * ----------------------------------------------------------------------------*/	
        	
            L2_parsingStruct.headerStruct.controlField = p_rxBuffer[controlPosition];

            L2_parsingStruct.packetType = L2_rxGetPacketType(p_rxBuffer,controlPosition,packetInfo);

            // The packet type is an internal translation of the type of the packet;
            // There is not a perfect equivalence with air format; we load this value
            // in the Control Header Struct beacause it's needed from the upper layers.
            L2_parsingStruct.headerStruct.dataType = (int)(L2_parsingStruct.packetType);

            switch( L2_parsingStruct.packetType )
            {
    	        case L2_P_NULL:          
                case L2_P_UNKNOWN_PACKET:
		        case L2_P_DATA_ACK:		// NOT IMPLEMENTED
		        case L2_P_DATA_NACK:	// NOT IMPLEMENTED
		        case L2_P_DATA_LOCK:	// NOT IMPLEMENTED		
                    
                    // dataPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
                    // These packet are unknown; discard them
                    return(WRONG_PACKET_TYPE);
                    
                //break;
        		
                case L2_P_ACK:
                case L2_P_NACK:
                case L2_P_LOCK:
                    
			        dataPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
			        L2_parsingStruct.sequenceCounter = PK_GET_SEQUENCE_COUNTER(p_rxBuffer,controlPosition);
                    // These packet cannot have other fields after the contol field; if so, discard them
                    if((L2_parsingStruct.packetLength - ( dataPosition - PK_LENGTH_FIELD_POSITION )) != 0)
                    {
                        return(WRONG_ACK_LENGTH);
                    }
			        //TODO: controllare, forse non serve
			        //L2_parsingStruct.directionBit = L2_GET_DIRECTION(p_rxBuffer,controlPosition);
                    
                break;
        		
		        case L2_P_DATA_WITHOUT_PTS:
		        case L2_P_DATA_WITH_PTS:
        								
			        L2_parsingStruct.sequenceCounter = PK_GET_SEQUENCE_COUNTER(p_rxBuffer,controlPosition);

                    frameSequence = PK_GET_FRAME_SEQUENCE(p_rxBuffer,controlPosition);

          	        if(	( frameSequence == L2_SINGLE_FRAME_PACKET ) ||
        		        ( frameSequence == L2_FIRST_FRAME_PACKET ) )
                    {
                        //TP header is present for SINGLE and FIRST packets of a frame
                        tpServiceInformationsPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
                        L2_parsingStruct.headerStruct.tpSAP = PK_GET_SAP(p_rxBuffer,tpServiceInformationsPosition);
                        L2_parsingStruct.headerStruct.tpSER = PK_GET_SERVICE_INFO(p_rxBuffer,tpServiceInformationsPosition);
    			        HS_SET_EXTENSIONS(L2_headerStructHandler,
                                          PK_GET_EXTENSIONS(p_rxBuffer,tpServiceInformationsPosition));
				        tpFramesIntegrityPosition = tpServiceInformationsPosition + L2_TP_SERVICE_INFO_FIELD_BYTES;
                        HS_SET_TP_INFOS(L2_headerStructHandler,
                                        PK_GET_TP_INFOS(p_rxBuffer,tpFramesIntegrityPosition));

                        L2_parsingStruct.headerStruct.CHU2.tpCounter =
                            L2_rxGetPacketTpCounter( p_rxBuffer, tpFramesIntegrityPosition );
                        dataPosition = controlPosition + PK_CONTROL_FIELD_BYTES + PK_TP_FIELD_BYTES;
                    }
                    else
                    {
                        // TP Header is not present for INTERMEDIATE and LAST packets of a frame
                        dataPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
                    }

			        //TODO: controllare, forse non serve
			        //L2_parsingStruct.directionBit = L2_GET_DIRECTION(p_rxBuffer,controlPosition);
        			            
                break;
            
	            case L2_P_SYNC_WITHOUT_PTS:
		        case L2_P_SYNC_WITH_PTS:
	            case L2_P_SYNC_ACK:
		        case L2_P_SYNC_CLR_WITHOUT_PTS:
		        case L2_P_SYNC_CLR_WITH_PTS:
	            case L2_P_SYNC_CLR_ACK:
	            case L2_P_SYNC_CFG_WITHOUT_PTS:// DEVO PRENDERMI I DATI DELLA CONFIGURAZIONE QUI
		        case L2_P_SYNC_CFG_WITH_PTS:// DEVO PRENDERMI I DATI DELLA CONFIGURAZIONE QUI
	            case L2_P_SYNC_CFG_ACK:// DEVO PRENDERMI I DATI DELLA CONFIGURAZIONE QUI

                    L2_parsingStruct.sequenceCounter = PK_GET_SEQUENCE_COUNTER(p_rxBuffer,controlPosition);
                    L2_parsingStruct.remoteCounter = PK_GET_REMOTE_COUNTER(p_rxBuffer,controlPosition); //New Sync
                    L2_parsingStruct.transportSyncPresent = PK_GET_SYNC_TP_PRESENCE(p_rxBuffer,controlPosition);
                    if( L2_parsingStruct.transportSyncPresent == TRUE_SS )
                    {
                        tpServiceInformationsPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
                        L2_parsingStruct.headerStruct.tpSAP = PK_GET_SAP(p_rxBuffer,tpServiceInformationsPosition);
                        L2_parsingStruct.headerStruct.tpSER = PK_GET_SERVICE_INFO(p_rxBuffer,tpServiceInformationsPosition);

                        HS_SET_EXTENSIONS( L2_headerStructHandler,
                                           PK_GET_EXTENSIONS(p_rxBuffer,tpServiceInformationsPosition));

                        tpFramesIntegrityPosition = tpServiceInformationsPosition + L2_TP_SERVICE_INFO_FIELD_BYTES;

                        HS_SET_TP_INFOS(L2_headerStructHandler,
                                        PK_GET_TP_INFOS(p_rxBuffer,tpFramesIntegrityPosition));

                        L2_parsingStruct.headerStruct.CHU2.tpCounter =
                            L2_rxGetPacketTpCounter( p_rxBuffer, tpFramesIntegrityPosition );
        				
				        if(	( L2_parsingStruct.packetType == L2_P_SYNC_CFG_WITHOUT_PTS ) || 
					        ( L2_parsingStruct.packetType == L2_P_SYNC_CFG_WITH_PTS ) )
				        {
					        //syncConfigPosition = controlPosition + PK_CONTROL_FIELD_BYTES + PK_TP_FIELD_BYTES + L2_SYNC_CFG_FIELD_POSITION;
                            syncConfigPosition = controlPosition + PK_CONTROL_FIELD_BYTES + PK_TP_FIELD_BYTES;
					        syncConfigLength = p_rxBuffer[syncConfigPosition];
                            //I campi che vengono supportati in maniera specifica da un protocollo,
                            //sono copiati nella parsing struct; gli altri non vengono processati grazie
                            //al campo lunghezza che specifica dove finiscono.
                            if( syncConfigLength > 1 )
                            {
                                L2_parsingStruct.maxBufferSize = p_rxBuffer[syncConfigPosition +
                                                                            L2_SYNC_CFG_MAX_BUFFERSIZE_POSITION];
                                L2_parsingStruct.maxBackoff = p_rxBuffer[syncConfigPosition +
                                                                        L2_SYNC_CFG_MAX_BACKOFF_POSITION];
                            }
					        dataPosition = syncConfigPosition + syncConfigLength;

				        }
				        else
				        {
					        //dataPosition = controlPosition + PK_CONTROL_FIELD_BYTES + L2_SYNC_CFG_FIELD_POSITION;
                            dataPosition = controlPosition + PK_CONTROL_FIELD_BYTES + PK_TP_FIELD_BYTES;
				        }
                    }
                    else
                    {
				        if(	( L2_parsingStruct.packetType == L2_P_SYNC_CFG_WITHOUT_PTS ) || 
					        ( L2_parsingStruct.packetType == L2_P_SYNC_CFG_WITH_PTS ) ||
					        ( L2_parsingStruct.packetType == L2_P_SYNC_CFG_ACK ) ) //maybe this is the only case
				        {
					        //syncConfigPosition = controlPosition + PK_CONTROL_FIELD_BYTES + L2_SYNC_CFG_FIELD_POSITION;
                            syncConfigPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
					        syncConfigLength = p_rxBuffer[syncConfigPosition];
                            //I campi che vengono supportati in maniera specifica da un protocollo,
                            //sono copiati nella parsing struct; gli altri non vengono processati grazie
                            //al campo lunghezza che specifica dove finiscono.
                            if( syncConfigLength > 1 )
                            {
                                L2_parsingStruct.maxBufferSize = p_rxBuffer[syncConfigPosition +
                                                                            L2_SYNC_CFG_MAX_BUFFERSIZE_POSITION];
                                L2_parsingStruct.maxBackoff = p_rxBuffer[syncConfigPosition +
                                                                        L2_SYNC_CFG_MAX_BACKOFF_POSITION];
                            }
					        dataPosition = syncConfigPosition + syncConfigLength;
				        }
				        else
				        {
					        dataPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
				        }			
                    }
        			
			        if( L2_parsingStruct.packetType == L2_P_SYNC_CLR_ACK )
			        {
                        // The CLR SYNC ACK packet adds various fields after the control
                        // - Compressed Address
                        // - Cradle Session Key
                        // - Cradle Global Status
                        // Old versions of Star Stack on gun cannot handle newer bytes;
                        // new versions of Star Stack on gun need to reset the variables containing newer features
                        // CLR SYNC ACK packet have no payload and no TP informations; after CONTROL field
                        // there are extra bytes for various informations. We check the packet lenght
                        // comparing it with the current position to extract the correct infomations.
                        
                        // compressed address and session key are always present                
				        L2_parsingStruct.compressedAddress = p_rxBuffer[dataPosition];
                        //L2_parsingStruct.sessionKey = p_rxBuffer[dataPosition+PK_CRADLE_KEY_ADDRESS_BYTES];
                        L2_parsingStruct.sessionKey = PK_GET_BEACON_SESSION_KEY(p_rxBuffer,(dataPosition+PK_CRADLE_KEY_ADDRESS_BYTES));
                        
                        L2_parsingStruct.globalStatus = PK_STATUS_READY;
                        dataPosition = controlPosition + PK_CONTROL_FIELD_BYTES + L2_COMPRESSED_ADDRESS_SIZE + PK_CRADLE_KEY_ADDRESS_BYTES;
                        
                        // check the presence of status byte                
                        if( L2_parsingStruct.packetLength > (  controlPosition +
                                                                PK_CONTROL_FIELD_BYTES +
                                                                L2_COMPRESSED_ADDRESS_SIZE +
                                                                PK_CRADLE_KEY_ADDRESS_BYTES -                                                        
                                                                PK_LENGTH_FIELD_POSITION ) )
                        {
                            // status present
                            L2_parsingStruct.globalStatus = PK_GET_BEACON_LOCK_STATUS(p_rxBuffer,(dataPosition+PK_CRADLE_STATUS_BYTES));
                            dataPosition += PK_CRADLE_STATUS_BYTES;
                        }
			        }
                    
                break;
            
                case L2_P_BEACON_PRESENCE:

                    L2_parsingStruct.sessionKey = PK_GET_BEACON_SESSION_KEY(p_rxBuffer,controlPosition);
                    globalStatusPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
                    L2_parsingStruct.globalStatus = PK_GET_BEACON_LOCK_STATUS(p_rxBuffer,globalStatusPosition);
                    dataPosition = globalStatusPosition + PK_BEACON_LOCK_STATUS_FIELD_BYTES;

                break;

                case L2_P_BEACON_INQUIRY:
                    
                    dataPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
                    
                break;
                
                case L2_P_BEACON_DATA:
                                
                    L2_parsingStruct.headerStruct.tpSAP = PK_GET_BEACON_SAP(p_rxBuffer,controlPosition);
                    counterPosition = controlPosition + PK_CONTROL_FIELD_BYTES;
                    L2_parsingStruct.beaconCounter = PK_GET_BEACON_COUNTER(p_rxBuffer,counterPosition);
                    cmdPosition = counterPosition + PK_BEACON_COUNTER_FIELD_BYTES;
                    paramPosition = cmdPosition + PK_BEACON_COMMAND_FIELD_BYTES;
                    L2_parsingStruct.headerStruct.CHU2.cmdExtension =
                        CTRL_MAKE_WORD(p_rxBuffer[paramPosition],p_rxBuffer[cmdPosition]);
                    //CTRL_MAKE_WORD(p_rxBuffer[cmdPosition],p_rxBuffer[paramPosition]);
			        //dataPosition = cmdPosition + PK_BEACON_PARAMETER_FIELD_BYTES;
			        dataPosition = paramPosition + PK_BEACON_PARAMETER_FIELD_BYTES;
                    
                break;
                
            }
        	
	        // Setting length of the data field in the parsing struct and in the headerStruct
            
	        //L2_parsingStruct.packetLength = packetLength - ( dataPosition - PK_LENGTH_FIELD_POSITION );
	        //L2_parsingStruct.headerStruct.payloadLen = L2_parsingStruct.packetLength;
            L2_parsingStruct.headerStruct.payloadLen = L2_parsingStruct.packetLength  -
                                                      ( dataPosition - PK_LENGTH_FIELD_POSITION );

            if(L2_parsingStruct.headerStruct.payloadLen > MAX_PACKET_LEN)
            {
                // wrong payload length: discard packet
                return(WRONG_PAYLOAD_LENGTH);
            }
        	
	        if(L2_parsingStruct.headerStruct.payloadLen == 0 )
            {
                L2_parsingStruct.dataBuffer = NULL;
            }
            else
            {
                // Copy mandatory for data alignment
                if((dataPosition & L2_PARSER_SIZE_T_REMAINDER_MASK) == 0)
                {
                    L2_parsingStruct.dataBuffer = &(p_rxBuffer[dataPosition]);
                }
                else
                {
                    for( i = 0; i < L2_parsingStruct.headerStruct.payloadLen; i++ )
                    {
                        //PAR_alignDataBuffer[i] = p_rxBuffer[dataPosition+i];
                        p_rxBuffer[i] = p_rxBuffer[dataPosition+i];
                    }
                    //L2_parsingStruct.dataBuffer = &(PAR_alignDataBuffer[0]);
                    L2_parsingStruct.dataBuffer = &(p_rxBuffer[0]);
                }
            }
        	
	        return(PACKET_CORRECT);
        	
	        //return &(L2_parsingStruct);
        	
        }

    }
}
