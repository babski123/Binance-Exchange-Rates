using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Net.Http;

namespace Binance_Exchange_Rates
{
    public partial class MainForm : Form
    {
        readonly HttpClient client = new HttpClient();
        private double USDtoPHPRate;
        private Point _mouseLoc;
        public MainForm()
        {
            InitializeComponent();
            Rectangle rectangle = Screen.PrimaryScreen.Bounds;
            Location = new Point(rectangle.Width - Width);
            _ = Main();
            Title.Text = "Loading data from Binance...";
        }

        public async Task Main()
        {
            string streamNames = "xrpusdt@ticker/btcusdt@ticker/ethusdt@ticker/bchusdt@ticker/dogeusdt@ticker/ltcusdt@ticker/adausdt@ticker/dotusdt@ticker/xlmusdt@ticker/bnbusdt@ticker/linkusdt@ticker/xmrusdt@ticker/degousdt@ticker";
            do
            {
                using (var socket = new ClientWebSocket())
                    try
                    {
                        await socket.ConnectAsync(new Uri("wss://stream.binance.com:9443/stream?streams=" + streamNames), CancellationToken.None);
                        await Receive(socket);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR - {ex.Message}");
                    }
            } while (true);
        }

        public async Task Receive(ClientWebSocket socket)
        {
            var buffer = new ArraySegment<byte>(new byte[2048]);
            do
            {
                WebSocketReceiveResult result;
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                        ConnectionStatus.Text = socket.State.ToString();
                        if (socket.State.ToString() == "Open")
                        {
                            ConnectionStatus.ForeColor = Color.GreenYellow;
                        }
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close) break;

                    ms.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        var receivedMsg = await reader.ReadToEndAsync();
                        StreamsPayload payload = JsonSerializer.Deserialize<StreamsPayload>(receivedMsg);
                        TickerStreamPayload data = payload.data;
                        Double lastPrice = Double.Parse(data.c) * USDtoPHPRate;
                        Double priceChangePercent = Double.Parse(data.P);
                        String lastPriceFormatted = "₱" + String.Format("{0:n}", lastPrice);
                        String priceChangePercentFormatted = String.Format("{0:n}", priceChangePercent) + "%";
                        Color priceChangePercentColor;
                        if (priceChangePercent < 0) priceChangePercentColor = ColorTranslator.FromHtml("#ED6042");
                        else if (priceChangePercent > 0) priceChangePercentColor = Color.GreenYellow;
                        else priceChangePercentColor = Title.ForeColor;

                        switch (data.s)
                        {
                            case "BTCUSDT":
                                BTCPRICE.Text = lastPriceFormatted;
                                BTCPERCENT.Text = priceChangePercentFormatted;
                                BTCPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "XRPUSDT":
                                XRPPRICE.Text = lastPriceFormatted;
                                XRPPERCENT.Text = priceChangePercentFormatted;
                                XRPPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "ETHUSDT":
                                ETHPRICE.Text = lastPriceFormatted;
                                ETHPERCENT.Text = priceChangePercentFormatted;
                                ETHPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "BCHUSDT":
                                BCHPRICE.Text = lastPriceFormatted;
                                BCHPERCENT.Text = priceChangePercentFormatted;
                                BCHPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "DOGEUSDT":
                                DOGEPRICE.Text = lastPriceFormatted;
                                DOGEPERCENT.Text = priceChangePercentFormatted;
                                DOGEPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "LTCUSDT":
                                LTCPRICE.Text = lastPriceFormatted;
                                LTCPERCENT.Text = priceChangePercentFormatted;
                                LTCPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "ADAUSDT":
                                ADAPRICE.Text = lastPriceFormatted;
                                ADAPERCENT.Text = priceChangePercentFormatted;
                                ADAPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "DOTUSDT":
                                DOTPRICE.Text = lastPriceFormatted;
                                DOTPERCENT.Text = priceChangePercentFormatted;
                                DOTPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "XLMUSDT":
                                XLMPRICE.Text = lastPriceFormatted;
                                XLMPERCENT.Text = priceChangePercentFormatted;
                                XLMPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "BNBUSDT":
                                BNBPRICE.Text = lastPriceFormatted;
                                BNBPERCENT.Text = priceChangePercentFormatted;
                                BNBPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "LINKUSDT":
                                LINKPRICE.Text = lastPriceFormatted;
                                LINKPERCENT.Text = priceChangePercentFormatted;
                                LINKPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "XMRUSDT":
                                XMRPRICE.Text = lastPriceFormatted;
                                XMRPERCENT.Text = priceChangePercentFormatted;
                                XMRPERCENT.ForeColor = priceChangePercentColor;
                                break;
                            case "DEGOUSDT":
                                DEGOPRICE.Text = lastPriceFormatted;
                                DEGOPERCENT.Text = priceChangePercentFormatted;
                                DEGOPERCENT.ForeColor = priceChangePercentColor;
                                break;
                        }

                        Title.Text = "Binance Cryptocurrency Rates";
                        this.Size = new Size(302,311);

                    }
                }
            } while (true);
        }

        private async void USDtoPHPChecker_Tick(object sender, EventArgs e)
        {
            string responseBody = await client.GetStringAsync("https://api.exchangeratesapi.io/latest?base=USD&symbols=PHP");
            CurrencyRatesData currencyRatesData = JsonSerializer.Deserialize<CurrencyRatesData>(responseBody);
            USDtoPHPRate = currencyRatesData.rates.PHP;
            USDPHP.Text = "1 USD = ₱" + String.Format("{0:n}", USDtoPHPRate);
            //Trace.WriteLine(socketRef.State);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseLoc = e.Location;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int dx = e.Location.X - _mouseLoc.X;
                int dy = e.Location.Y - _mouseLoc.Y;
                this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
            }
        }

        private void MainForm_MouseEnter(object sender, EventArgs e)
        {
            Opacity = 1.0;
        }

        private void MainForm_MouseLeave(object sender, EventArgs e)
        {
            Opacity = 0.5;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
        }

        class StreamsPayload
        {
            public string stream { get; set; }
            public TickerStreamPayload data { get; set; }
        }

        class TickerStreamPayload
        {
            public string e { get; set; } //event type
            public long E { get; set; } //event time
            public string s { get; set; } //symbol
            public string p { get; set; } //price change
            public string P { get; set; } //price change percent
            public string w { get; set; } //weighted average price
            public string x { get; set; } //first trade price for the last 24 hour rolling window
            public string c { get; set; } //last price
            public string Q { get; set; } //last quantity
            public string b { get; set; } //best bid price
            public string B { get; set; } //best bid quantity
            public string a { get; set; } //best ask price
            public string A { get; set; } //best ask quantity
            public string o { get; set; } //open price
            public string h { get; set; } //high price
            public string l { get; set; } //low price
            public string v { get; set; } //total traded base asset volume
            public string q { get; set; } //total traded quote asset volume
            public long O { get; set; } //statistics open time
            public long C { get; set; } //statistics close time
            public int F { get; set; } //first trade ID
            public int L { get; set; } //last trade ID
            public int n { get; set; } //total number of trades
        }

        class CurrencyRatesData
        {
            public Currency rates { get; set; }
        }

        class Currency
        {
         public double PHP { get; set; }
        }
    }
}
