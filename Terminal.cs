/* 
 * Project:    SerialPort Terminal
 * Company:    Coad .NET, http://coad.net
 * Author:     Noah Coad, http://coad.net/noah
 * Created:    March 2005
 * 
 * Notes:      This was created to demonstrate how to use the SerialPort control for
 *             communicating with your PC's Serial RS-232 COM Port
 * 
 *             It is for educational purposes only and not sanctified for industrial use. :)
 *             Written to support the blog post article at: http://msmvps.com/blogs/coad/archive/2005/03/23/39466.aspx
 * 
 *             Search for "comport" to see how I'm using the SerialPort control.
 */

#region Namespace Inclusions
using System;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

using SerialPortTerminal.Properties;
using System.Threading;
using System.IO;
#endregion

namespace SerialPortTerminal
{
  #region Public Enumerations
  public enum DataMode { Text, Hex }
  public enum LogMsgType { Incoming, Outgoing, Normal, Warning, Error };
  #endregion

  public partial class frmTerminal : Form
  {
	#region Local Variables

	// The main control for communicating through the RS-232 port
	private SerialPort comport = new SerialPort();

	// Various colors for logging info
	private Color[] LogMsgTypeColor = { Color.Blue, Color.Green, Color.Black, Color.Orange, Color.Red };

	// Temp holder for whether a key was pressed
	private bool KeyHandled = false;

		private Settings settings = Settings.Default;
	#endregion

    int[] packetNumber = new int[3];
    int[] prevPacketNumber = new int[3];
    int[] packetData = new int[3];
	int[] lostPackets = new int[3];
	int[] packetTime = new int[3];

	// format is \r\n000:0548:1023:0515:\r\n
    // 0D 0A 30 30 30 3A 30 30 32 30 3A 30 31 33 33 3A 31 30 32 33 3A 0D 0A
    bool startDetected = false;
    int nodeNo = -2;
    int packetIndex = 0;
    byte[] packet = new byte[23];

    //StreamWriter w;

    //bool log;


	#region Constructor
	public frmTerminal()
	{

			// Load user settings
		settings.Reload();

	  // Build the form
	  InitializeComponent();

	  // Restore the users settings
	  InitializeControlValues();

	  // Enable/disable controls based on the current state
	  EnableControls();

	  // When data is recieved through the port, call this method
	  comport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
			comport.PinChanged += new SerialPinChangedEventHandler(comport_PinChanged);
	
	}

	  

		void comport_PinChanged(object sender, SerialPinChangedEventArgs e)
		{
			// Show the state of the pins
			UpdatePinState();
		}

		private void UpdatePinState()
		{
			this.Invoke(new ThreadStart(() => {
				// Show the state of the pins
				chkCD.Checked = comport.CDHolding;
				chkCTS.Checked = comport.CtsHolding;
				chkDSR.Checked = comport.DsrHolding;
			}));
		}
	#endregion

	#region Local Methods
	
	/// <summary> Save the user's settings. </summary>
	private void SaveSettings()
	{
			settings.BaudRate = int.Parse(cmbBaudRate.Text);
			settings.DataBits = int.Parse(cmbDataBits.Text);
			settings.DataMode = CurrentDataMode;
			settings.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
			settings.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);
			settings.PortName = cmbPortName.Text;
			settings.ClearOnOpen = chkClearOnOpen.Checked;
			settings.ClearWithDTR = chkClearWithDTR.Checked;

			settings.Save();
	}

	/// <summary> Populate the form's controls with default settings. </summary>
	private void InitializeControlValues()
	{
	  cmbParity.Items.Clear(); cmbParity.Items.AddRange(Enum.GetNames(typeof(Parity)));
	  cmbStopBits.Items.Clear(); cmbStopBits.Items.AddRange(Enum.GetNames(typeof(StopBits)));

			cmbParity.Text = settings.Parity.ToString();
			cmbStopBits.Text = settings.StopBits.ToString();
			cmbDataBits.Text = settings.DataBits.ToString();
			cmbParity.Text = settings.Parity.ToString();
			cmbBaudRate.Text = settings.BaudRate.ToString();
			CurrentDataMode = settings.DataMode;

			RefreshComPortList();

			chkClearOnOpen.Checked = settings.ClearOnOpen;
			chkClearWithDTR.Checked = settings.ClearWithDTR;

			// If it is still avalible, select the last com port used
			if (cmbPortName.Items.Contains(settings.PortName)) cmbPortName.Text = settings.PortName;
	  else if (cmbPortName.Items.Count > 0) cmbPortName.SelectedIndex = cmbPortName.Items.Count - 1;
	  else
	  {
		MessageBox.Show(this, "There are no COM Ports detected on this computer.\nPlease install a COM Port and restart this app.", "No COM Ports Installed", MessageBoxButtons.OK, MessageBoxIcon.Error);
		this.Close();
	  }
	}

	/// <summary> Enable/disable controls based on the app's current state. </summary>
	private void EnableControls()
	{
	  // Enable/disable controls based on whether the port is open or not
	  gbPortSettings.Enabled = !comport.IsOpen;
	  txtSendData.Enabled = btnSend.Enabled = comport.IsOpen;
			//chkDTR.Enabled = chkRTS.Enabled = comport.IsOpen;

	  if (comport.IsOpen) btnOpenPort.Text = "&Close Port";
	  else btnOpenPort.Text = "&Open Port";
	}

	/// <summary> Send the user's data currently entered in the 'send' box.</summary>
	private void SendData()
	{
	  if (CurrentDataMode == DataMode.Text)
	  {
		  if (chkbox_crlf.Checked == true && checkBox1.Checked == true)
		  {
			  // Send the user's text straight out the port
			  comport.Write(txtSendData.Text + "\r\n");
		  }
		  else if (chkbox_crlf.Checked == true && checkBox1.Checked == false)
		  {
			  // Send the user's text straight out the port
			  comport.Write(txtSendData.Text + "\n");
		  }
		  else if (chkbox_crlf.Checked == false && checkBox1.Checked == true)
		  {
			  // Send the user's text straight out the port
			  comport.Write(txtSendData.Text + "\r");
		  }
		  else {
			  // Send the user's text straight out the port
			  comport.Write(txtSendData.Text);
		  }

	  
		// Show in the terminal window the user's text
		Log(LogMsgType.Outgoing, txtSendData.Text + "\n");
	  }
	  else
	  {
		try
		{
		  // Convert the user's string of hex digits (ex: B4 CA E2) to a byte array
		  byte[] data = HexStringToByteArray(txtSendData.Text);

		  // Send the binary data out the port
		  comport.Write(data, 0, data.Length);

		  // Show the hex digits on in the terminal window
		  Log(LogMsgType.Outgoing, ByteArrayToHexString(data) + "\n");
		}
		catch (FormatException)
		{
		  // Inform the user if the hex string was not properly formatted
		  Log(LogMsgType.Error, "Not properly formatted hex string: " + txtSendData.Text + "\n");
		}
	  }
	  txtSendData.SelectAll();
	}

	/// <summary> Log data to the terminal window. </summary>
	/// <param name="msgtype"> The type of message to be written. </param>
	/// <param name="msg"> The string containing the message to be shown. </param>
	private void Log(LogMsgType msgtype, string msg)
	{
	  rtfTerminal.Invoke(new EventHandler(delegate
	  {
		rtfTerminal.SelectedText = string.Empty;
		rtfTerminal.SelectionFont = new Font(rtfTerminal.SelectionFont, FontStyle.Bold);
		rtfTerminal.SelectionColor = LogMsgTypeColor[(int)msgtype];
		rtfTerminal.AppendText(msg);
		rtfTerminal.ScrollToCaret();
	  }));
	}

	/// <summary> Convert a string of hex digits (ex: E4 CA B2) to a byte array. </summary>
	/// <param name="s"> The string containing the hex digits (with or without spaces). </param>
	/// <returns> Returns an array of bytes. </returns>
	private byte[] HexStringToByteArray(string s)
	{
	  s = s.Replace(" ", "");
	  byte[] buffer = new byte[s.Length / 2];
	  for (int i = 0; i < s.Length; i += 2)
		buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
	  return buffer;
	}

	/// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
	/// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
	/// <returns> Returns a well formatted string of hex digits with spacing. </returns>
	private string ByteArrayToHexString(byte[] data)
	{
	  StringBuilder sb = new StringBuilder(data.Length * 3);
	  foreach (byte b in data)
		sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
	  return sb.ToString().ToUpper();
	}
	#endregion

	#region Local Properties
	private DataMode CurrentDataMode
	{
	  get
	  {
		if (rbHex.Checked) return DataMode.Hex;
		else return DataMode.Text;
	  }
	  set
	  {
		if (value == DataMode.Text) rbText.Checked = true;
		else rbHex.Checked = true;
	  }
	}
	#endregion

	#region Event Handlers
	private void lnkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
	  // Show the user the about dialog
	  (new frmAbout()).ShowDialog(this);
	}
	
	private void frmTerminal_Shown(object sender, EventArgs e)
	{
	  Log(LogMsgType.Normal, String.Format("Application Started at {0}\n", DateTime.Now));
	}
	private void frmTerminal_FormClosing(object sender, FormClosingEventArgs e)
	{
	  // The form is closing, save the user's preferences
	  SaveSettings();
	}

	private void rbText_CheckedChanged(object sender, EventArgs e)
	{ if (rbText.Checked) CurrentDataMode = DataMode.Text; }

	private void rbHex_CheckedChanged(object sender, EventArgs e)
	{ if (rbHex.Checked) CurrentDataMode = DataMode.Hex; }

	private void cmbBaudRate_Validating(object sender, CancelEventArgs e)
	{ int x; e.Cancel = !int.TryParse(cmbBaudRate.Text, out x); }

	private void cmbDataBits_Validating(object sender, CancelEventArgs e)
	{ int x; e.Cancel = !int.TryParse(cmbDataBits.Text, out x); }

	private void btnOpenPort_Click(object sender, EventArgs e)
	{
			bool error = false;

	  // If the port is open, close it.
			if (comport.IsOpen)
			{
				comport.Close();
				//w.Close();
			}
			else
			{
				// Set the port's settings
				comport.BaudRate = int.Parse(cmbBaudRate.Text);
				comport.DataBits = int.Parse(cmbDataBits.Text);
				comport.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cmbStopBits.Text);
				comport.Parity = (Parity)Enum.Parse(typeof(Parity), cmbParity.Text);
				comport.PortName = cmbPortName.Text;

				try
				{
					// Open the port
					comport.Open();
                    //if (log == true) {
                    //    w.Write("\r\nLog Entry : ");
                    //    w.WriteLine("{0} {1}",
                    //        DateTime.Now.ToLongTimeString(),
                    //        DateTime.Now.ToLongDateString());                    
                    //}
				}
				catch (UnauthorizedAccessException) { error = true; }
				catch (IOException) { error = true; }
				catch (ArgumentException) { error = true; }

				if (error) MessageBox.Show(this, "Could not open the COM port.  Most likely it is already in use, has been removed, or is unavailable.", "COM Port Unavalible", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				else
				{
					// Show the initial pin states
					UpdatePinState();
					chkDTR.Checked = comport.DtrEnable;
					chkRTS.Checked = comport.RtsEnable;
				}
			}

	  // Change the state of the form's controls
	  EnableControls();

	  // If the port is open, send focus to the send data box
			if (comport.IsOpen)
			{
				txtSendData.Focus();
				if (chkClearOnOpen.Checked) ClearTerminal();
			}
	}

	private void btnSend_Click(object sender, EventArgs e)
	{ SendData(); }

	private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		// If the com port has been closed, do nothing
		if (!comport.IsOpen) return;

	  // This method will be called when there is data waiting in the port's buffer

	  // Determain which mode (string or binary) the user is in
	  if (CurrentDataMode == DataMode.Text)
	  {
		// Read all the data waiting in the buffer
		string data = comport.ReadExisting();

		updateLabels2(data);

		// Display the text to the user in the terminal
		Log(LogMsgType.Incoming, data);
	  }
	  else
	  {
		// Obtain the number of bytes waiting in the port's buffer
		int bytes = comport.BytesToRead;

		// Create a byte array buffer to hold the incoming data
		byte[] buffer = new byte[bytes];

		// Read the data from the port and store it in our buffer
		comport.Read(buffer, 0, bytes);

		// Show the user the incoming data in hex format
		Log(LogMsgType.Incoming, ByteArrayToHexString(buffer));
	  }
	}

	private void updateLabels(string data)
	{
        int z = 0;
        byte a = 0x00;
        while (z < data.Length)
        {
            a = Convert.ToByte(data[z++]);
            if (startDetected)
                packet[packetIndex++] = a;
            else
            {
                if (a == '\r')
                    packet[0] = a;
                if (a == '\n')
                    packet[1] = a;
                if (packet[0] == '\r' && packet[1] == '\n')
                {
                    startDetected = true;
                    packetIndex = 2;
                    for (int tmp = 2; tmp < 23; tmp++)
                        packet[tmp] = 0;
                }
            }
            if (startDetected == true && packet[0] == '\r' && packet[1] == '\n' && packet[2] == '\r')
            {
                for (int tmp = 2; tmp < 23; tmp++)
                    packet[tmp] = 0;
                startDetected = false;
                packet[2] = 0;
                packetIndex = 0;
            }

            if (packetIndex == 22)
            if (startDetected == true
                && packet[0] == '\r' && packet[1] == '\n'
                && packet[21] == '\r' && packet[22] == '\n')
            {
                if (packet[2] == 0x30 && packet[3] == 0x30 && packet[4] == 0x30)
                    nodeNo = 0;
                else if (packet[2] == 0x31 && packet[3] == 0x31 && packet[4] == 0x31)
                    nodeNo = 1;
                else if (packet[2] == 0x32 && packet[3] == 0x32 && packet[4] == 0x32)
                    nodeNo = 2;

                if (nodeNo == 0 || nodeNo == 1 || nodeNo == 2)
                {
                    packetNumber[nodeNo] = packet[6] * 1000 + packet[7] * 100 + packet[8] * 10 + packet[9] * 1;
                    packetTime[nodeNo] = packet[11] * 1000 + packet[12] * 100 + packet[13] * 10 + packet[14] * 1;
                    packetData[nodeNo] = packet[16] * 1000 + packet[17] * 100 + packet[18] * 10 + packet[19] * 1;
                    for (int tmp = 0; tmp < 23; tmp++)
                        packet[tmp] = 0;
                    packetIndex = 0;
                }
                packetIndex = 0;
            }
            else
                packetIndex = 0;
        }

		lbl_PcktNo_0.Invoke(new EventHandler(delegate
		{
			lbl_PcktNo_0.Text = packetNumber[0].ToString();
		}));
		lbl_PcktData_0.Invoke(new EventHandler(delegate
		{
			lbl_PcktData_0.Text = packetData[0].ToString();
		}));
		lbl_LostPckt_0.Invoke(new EventHandler(delegate
		{
			lbl_LostPckt_0.Text = lostPackets[0].ToString();
		}));
		lbl_PcktTime_0.Invoke(new EventHandler(delegate
		{
			lbl_PcktTime_0.Text = packetTime[0].ToString();
		}));


		lbl_PcktNo_1.Invoke(new EventHandler(delegate
		{
			lbl_PcktNo_1.Text = packetNumber[1].ToString();
		}));
		lbl_PcktData_1.Invoke(new EventHandler(delegate
		{
			lbl_PcktData_1.Text = packetData[1].ToString();
		}));
		lbl_LostPckt_1.Invoke(new EventHandler(delegate
		{
			lbl_LostPckt_1.Text = lostPackets[1].ToString();
		}));
		lbl_PcktTime_1.Invoke(new EventHandler(delegate
		{
			lbl_PcktTime_1.Text = packetTime[1].ToString();
		}));


		lbl_PcktNo_2.Invoke(new EventHandler(delegate
		{
			lbl_PcktNo_2.Text = packetNumber[2].ToString();
		}));
		lbl_PcktData_2.Invoke(new EventHandler(delegate
		{
			lbl_PcktData_2.Text = packetData[2].ToString();
		}));
		lbl_LostPckt_2.Invoke(new EventHandler(delegate
		{
			lbl_LostPckt_2.Text = lostPackets[2].ToString();
		}));
		lbl_PcktTime_2.Invoke(new EventHandler(delegate
		{
			lbl_PcktTime_2.Text = packetTime[2].ToString();
		}));

		//lbl_TotalPckts.Text = (packetNumber[0] + packetNumber[1] + packetNumber[2]).ToString();
		//lbl_LostPckts.Text = (lostPackets[0] + lostPackets[1] + lostPackets[2]).ToString();
	}

    private void updateLabels2(string data)
    {
        int z = 0;

        while (packetIndex == 0 && z < data.Length)
        {
            packet[packetIndex] = Convert.ToByte(data[z++]); ;
            if (packet[packetIndex] == '\r')
                packetIndex++;
        }

        while (packetIndex == 1 && z<data.Length)
        {
            packet[packetIndex] = Convert.ToByte(data[z++]); ;
            if (packet[packetIndex] == '\n')
                packetIndex++;
        }

        while (packetIndex > 1 && packetIndex < 23 && z < data.Length)
        {
            packet[packetIndex] = Convert.ToByte(data[z++]); ;
            if(packet[2]!='\r' && packet[2]!='\n')
                packetIndex++;
        }

        if (packetIndex == 23)
        {
            if (packet[0] == '\r' && packet[1] == '\n' && packet[21] == '\r' && packet[22] == '\n')
            {
                if (packet[2] == '0' && packet[3] == '0' && packet[4] == '0')
                    nodeNo = 0;
                else if (packet[2] == '1' && packet[3] == '1' && packet[4] == '1')
                    nodeNo = 1;
                else if (packet[2] == '2' && packet[3] == '2' && packet[4] == '2')
                    nodeNo = 2;

                if (nodeNo == 0 || nodeNo == 1 || nodeNo == 2)
                {
                    packetNumber[nodeNo] = (packet[6] - 48) * 1000 + (packet[7] - 48) * 100 + (packet[8] - 48) * 10 + (packet[9] - 48) * 1;
                    packetTime[nodeNo] = (packet[11] - 48) * 1000 + (packet[12] - 48) * 100 + (packet[13] - 48) * 10 + (packet[14] - 48) * 1;
                    packetData[nodeNo] = (packet[16] - 48) * 1000 + (packet[17] - 48) * 100 + (packet[18] - 48) * 10 + (packet[19] - 48) * 1;
                    for (int tmp = 0; tmp < 23; tmp++)
                        packet[tmp] = 0;
                    packetIndex = 0;
                    nodeNo = -2;                    
                    if(packetNumber[0]==1){
                        for (z = 0; z < 3; z++)
                        {
                            packetNumber[z] = 0;
                            prevPacketNumber[z] = 0;
                        }
                    }
                }
            }
            else
            {
                for (int tmp = 0; tmp < 23; tmp++)
                    packet[tmp] = 0;
                packetIndex = 0;
                nodeNo = -2;
            }
        }

        for (z = 0; z < 3; z++)
        {
            if (packetNumber[z] - prevPacketNumber[z] > 1)
                lostPackets[z] = packetNumber[z] - prevPacketNumber[z];
            prevPacketNumber[z] = packetNumber[z];
        }

        lbl_PcktNo_0.Invoke(new EventHandler(delegate
        {
            lbl_PcktNo_0.Text = packetNumber[0].ToString();
        }));
        lbl_PcktData_0.Invoke(new EventHandler(delegate
        {
            lbl_PcktData_0.Text = packetData[0].ToString();
        }));
        lbl_LostPckt_0.Invoke(new EventHandler(delegate
        {
            lbl_LostPckt_0.Text = lostPackets[0].ToString();
        }));
        lbl_PcktTime_0.Invoke(new EventHandler(delegate
        {
            lbl_PcktTime_0.Text = packetTime[0].ToString();
        }));


        lbl_PcktNo_1.Invoke(new EventHandler(delegate
        {
            lbl_PcktNo_1.Text = packetNumber[1].ToString();
        }));
        lbl_PcktData_1.Invoke(new EventHandler(delegate
        {
            lbl_PcktData_1.Text = packetData[1].ToString();
        }));
        lbl_LostPckt_1.Invoke(new EventHandler(delegate
        {
            lbl_LostPckt_1.Text = lostPackets[1].ToString();
        }));
        lbl_PcktTime_1.Invoke(new EventHandler(delegate
        {
            lbl_PcktTime_1.Text = packetTime[1].ToString();
        }));


        lbl_PcktNo_2.Invoke(new EventHandler(delegate
        {
            lbl_PcktNo_2.Text = packetNumber[2].ToString();
        }));
        lbl_PcktData_2.Invoke(new EventHandler(delegate
        {
            lbl_PcktData_2.Text = packetData[2].ToString();
        }));
        lbl_LostPckt_2.Invoke(new EventHandler(delegate
        {
            lbl_LostPckt_2.Text = lostPackets[2].ToString();
        }));
        lbl_PcktTime_2.Invoke(new EventHandler(delegate
        {
            lbl_PcktTime_2.Text = packetTime[2].ToString();
        }));

        lbl_TotalPckts.Invoke(new EventHandler(delegate
        {
            lbl_TotalPckts.Text = (packetNumber[0] + packetNumber[1] + packetNumber[2] - lostPackets[0] + lostPackets[1] + lostPackets[2]).ToString();
        }));
        lbl_LostPckts.Invoke(new EventHandler(delegate
        {
            lbl_LostPckts.Text = (lostPackets[0] + lostPackets[1] + lostPackets[2]).ToString();
        }));
    }


	private void txtSendData_KeyDown(object sender, KeyEventArgs e)
	{ 
	  // If the user presses [ENTER], send the data now
	  if (KeyHandled = e.KeyCode == Keys.Enter) { e.Handled = true; SendData(); } 
	}

	private void txtSendData_KeyPress(object sender, KeyPressEventArgs e)
	{ e.Handled = KeyHandled; }
	#endregion

		private void chkDTR_CheckedChanged(object sender, EventArgs e)
		{
			comport.DtrEnable = chkDTR.Checked;
			if (chkDTR.Checked && chkClearWithDTR.Checked) ClearTerminal();
		}

		private void chkRTS_CheckedChanged(object sender, EventArgs e)
		{
			comport.RtsEnable = chkRTS.Checked;
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			ClearTerminal();
		}

		private void ClearTerminal()
		{
			rtfTerminal.Clear();
		}

		private void tmrCheckComPorts_Tick(object sender, EventArgs e)
		{
			// checks to see if COM ports have been added or removed
			// since it is quite common now with USB-to-Serial adapters
			RefreshComPortList();
		}

		private void RefreshComPortList()
		{
			// Determain if the list of com port names has changed since last checked
			string selected = RefreshComPortList(cmbPortName.Items.Cast<string>(), cmbPortName.SelectedItem as string, comport.IsOpen);

			// If there was an update, then update the control showing the user the list of port names
			if (!String.IsNullOrEmpty(selected))
			{
				cmbPortName.Items.Clear();
				cmbPortName.Items.AddRange(OrderedPortNames());
				cmbPortName.SelectedItem = selected;
			}
		}

		private string[] OrderedPortNames()
		{
			// Just a placeholder for a successful parsing of a string to an integer
			int num;

			// Order the serial port names in numberic order (if possible)
			return SerialPort.GetPortNames().OrderBy(a => a.Length > 3 && int.TryParse(a.Substring(3), out num) ? num : 0).ToArray(); 
		}
		
		private string RefreshComPortList(IEnumerable<string> PreviousPortNames, string CurrentSelection, bool PortOpen)
		{
			// Create a new return report to populate
			string selected = null;

			// Retrieve the list of ports currently mounted by the operating system (sorted by name)
			string[] ports = SerialPort.GetPortNames();

			// First determain if there was a change (any additions or removals)
			bool updated = PreviousPortNames.Except(ports).Count() > 0 || ports.Except(PreviousPortNames).Count() > 0;

			// If there was a change, then select an appropriate default port
			if (updated)
			{
				// Use the correctly ordered set of port names
				ports = OrderedPortNames();

				// Find newest port if one or more were added
				string newest = SerialPort.GetPortNames().Except(PreviousPortNames).OrderBy(a => a).LastOrDefault();

				// If the port was already open... (see logic notes and reasoning in Notes.txt)
				if (PortOpen)
				{
					if (ports.Contains(CurrentSelection)) selected = CurrentSelection;
					else if (!String.IsNullOrEmpty(newest)) selected = newest;
					else selected = ports.LastOrDefault();
				}
				else
				{
					if (!String.IsNullOrEmpty(newest)) selected = newest;
					else if (ports.Contains(CurrentSelection)) selected = CurrentSelection;
					else selected = ports.LastOrDefault();
				}
			}

			// If there was a change to the port list, return the recommended default selection
			return selected;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			updateLabels("something something something");
		}

        private void button2_Click_1(object sender, EventArgs e)
        {
            for (int z = 0; z < 3; z++)
            {
                packetNumber[z] = 0;
                prevPacketNumber[z] = 0;
            }
        }
	}
}