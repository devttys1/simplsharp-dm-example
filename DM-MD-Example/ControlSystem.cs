using System;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DM;

namespace DM_MD_Example
{
    public class ControlSystem : CrestronControlSystem
    {
        public XpanelForSmartGraphics xp;
        public DmMd8x8 matrix;
        /// <summary>
        /// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// 
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// 
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;

                xp = new XpanelForSmartGraphics(0xaa, this);
                xp.SigChange += new SigEventHandler(xp_SigChange);
                xp.OnlineStatusChange += new OnlineStatusChangeEventHandler(xp_OnlineStatusChange);
                xp.Register();

                matrix = new DmMd8x8(0x20, this);
                matrix.DMInputChange += new DMInputEventHandler(matrix_DMInputChange);
                matrix.DMOutputChange += new DMOutputEventHandler(matrix_DMOutputChange);
                matrix.OnlineStatusChange += new OnlineStatusChangeEventHandler(matrix_OnlineStatusChange);
                matrix.Register();

                //Subscribe to the controller events (System, Program, and Ethernet)
                CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControlSystem_ControllerSystemEventHandler);
                CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControlSystem_ControllerProgramEventHandler);
                CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControlSystem_ControllerEthernetEventHandler);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        void matrix_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (currentDevice.IsOnline)
            {
                CrestronConsole.PrintLine("DM-MD-8x8 came Online");
            }
            else if (currentDevice.IsOnline == false)
            {
                CrestronConsole.PrintLine("DM-MD-8x8 went Offline");
            }
        }

        void xp_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (currentDevice.IsOnline)
            {
                CrestronConsole.PrintLine("xPanel came Online");
            }
        }

        void matrix_DMOutputChange(Switch device, DMOutputEventArgs args)
        {
            uint n = args.Number;
            uint x = (uint)device.NumberOfOutputs;

            for (uint i = 1; i <= x; i++)
            {
                xp.BooleanInput[i + ((n-1) * x)].BoolValue = false;
            }
            xp.BooleanInput[device.Outputs[n].VideoOutFeedback.Number + (n-1) * x].BoolValue = true;
            CrestronConsole.PrintLine("Switched input " + (device.Outputs[n].VideoOutFeedback.Number) + " to output " + n + ".");
        }

        void matrix_DMInputChange(Switch device, DMInputEventArgs args)
        {
            if (device.Inputs[args.Number].VideoDetectedFeedback.BoolValue == true)
            {
                CrestronConsole.PrintLine("Video Detected on Input " + args.Number + ".");
            }
        }

        void xp_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            // determine what type of sig has changed
            switch (args.Sig.Type)
            {
                // a bool (digital) has changed
                case eSigType.Bool:
                    // determine if the bool sig is true (digital high, press) or false (digital low, release)
                    if (args.Sig.BoolValue)		// press
                    {
                        matrix.VideoEnter.BoolValue = true;
                        uint i = args.Sig.Number; // determine what sig (join) number has chagned
                        uint x = (uint)matrix.NumberOfOutputs; //number of outputs
                        uint n = (i / x)+1; //find which output that corresponds to the join number

                        matrix.Outputs[n].VideoOut = matrix.Inputs[i - ((n-1) * x)]; //route input to output
                        CrestronConsole.PrintLine("Switching input " + (i - ((n-1) * x)) + " to output " + n + ".");
                       
                        #region switch case
                        /*
                        switch (args.Sig.Number)
                        {
                            case 1:
                                matrix.Outputs[1].VideoOut = matrix.Inputs[1];
                                
                                break;

                            case 2:
                                matrix.Outputs[1].VideoOut = matrix.Inputs[2];
                                break;

                            case 3:
                                matrix.Outputs[1].VideoOut = matrix.Inputs[3];
                                break;

                            case 4:
                                matrix.Outputs[1].VideoOut = matrix.Inputs[4];
                                break;

                            default:
                                break;
                        } */
                        #endregion
                    }
                    else						// release
                    {
                        #region switch case
                        // determine what sig (join) number has changed
                        /*
                        switch (args.Sig.Number)
                        {
                            case 3:
                                
                                break;

                            case 4:
                                
                                break;

                            default:
                                break;
                        } */
                        #endregion
                    }

                    break;

                // a ushort (analog) has chagned
                case eSigType.UShort:
                    switch (args.Sig.Number)
                    {
                        case 1:
                            // send the slider value to the lamp dimmer
                            //lampDimmer.DimmingLoads[1].Level.UShortValue = args.Sig.UShortValue;
                            break;

                        default:
                            break;
                    }
                    break;

                case eSigType.String:
                case eSigType.NA:
                default:
                    break;
            }
        }

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            try
            {

            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        /// <summary>
        /// Event Handler for Ethernet events: Link Up and Link Down. 
        /// Use these events to close / re-open sockets, etc. 
        /// </summary>
        /// <param name="ethernetEventArgs">This parameter holds the values 
        /// such as whether it's a Link Up or Link Down event. It will also indicate 
        /// wich Ethernet adapter this event belongs to.
        /// </param>
        void ControlSystem_ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            switch (ethernetEventArgs.EthernetEventType)
            {//Determine the event type Link Up or Link Down
                case (eEthernetEventType.LinkDown):
                    //Next need to determine which adapter the event is for. 
                    //LAN is the adapter is the port connected to external networks.
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {
                        //
                    }
                    break;
                case (eEthernetEventType.LinkUp):
                    if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
                    {

                    }
                    break;
            }
        }

        /// <summary>
        /// Event Handler for Programmatic events: Stop, Pause, Resume.
        /// Use this event to clean up when a program is stopping, pausing, and resuming.
        /// This event only applies to this SIMPL#Pro program, it doesn't receive events
        /// for other programs stopping
        /// </summary>
        /// <param name="programStatusEventType"></param>
        void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Paused):
                    //The program has been paused.  Pause all user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Resumed):
                    //The program has been resumed. Resume all the user threads/timers as needed.
                    break;
                case (eProgramStatusEventType.Stopping):
                    //The program has been stopped.
                    //Close all threads. 
                    //Shutdown all Client/Servers in the system.
                    //General cleanup.
                    //Unsubscribe to all System Monitor events
                    break;
            }

        }

        /// <summary>
        /// Event Handler for system events, Disk Inserted/Ejected, and Reboot
        /// Use this event to clean up when someone types in reboot, or when your SD /USB
        /// removable media is ejected / re-inserted.
        /// </summary>
        /// <param name="systemEventType"></param>
        void ControlSystem_ControllerSystemEventHandler(eSystemEventType systemEventType)
        {
            switch (systemEventType)
            {
                case (eSystemEventType.DiskInserted):
                    //Removable media was detected on the system
                    break;
                case (eSystemEventType.DiskRemoved):
                    //Removable media was detached from the system
                    break;
                case (eSystemEventType.Rebooting):
                    //The system is rebooting. 
                    //Very limited time to preform clean up and save any settings to disk.
                    break;
            }

        }
    }
}