namespace HVO.PowerMonitor.V1.HostedServices.InverterService
{
    public partial class InverterServiceProcessor
    {
        public async Task<(bool IsSuccess, string ProtocolID)> QPI(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QPI");
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QPI", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));

                var s1 = System.Text.Encoding.ASCII.GetString(response.Data.Span);
                return (true, s1);
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string SerialNumber)> QID(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QID");
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QID", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));

                var s1 = System.Text.Encoding.ASCII.GetString(response.Data.Span);
                return (true, s1);
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string SerialNumber)> QSID(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QSID");
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QSID", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));

                // The first 2 bytes are the lenght of the serial number
                var s1 = System.Text.Encoding.ASCII.GetString(response.Data.Span);
                return (true, s1);
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> QVFW(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QVFW"); // 00 51 56 46 57 62 99 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QVFW", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));

                return (true, Version.Parse("0.0").ToString());
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> QVFW2(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QVFW2"); //00 51 56 46 57 32 C3 F5 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QVFW2", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));

                return (true, Version.Parse("0.0").ToString());
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> QVFW3(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QVFW3"); // 00 51 56 46 57 33 D3 D4 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QVFW3", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));

                return (true, Version.Parse("0.0").ToString());
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> VERFW(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("VERFW"); // 00 56 45 52 46 57 B7 B8 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "VERFW", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));

                return (true, Version.Parse("0.0").ToString());
            }
            else
            {
                Console.WriteLine($"FAIL: VERFW");
            }

            await Task.Yield();
            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, object Model)> QPIRI(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QPIRI"); // 00 51 50 49 52 49 F8 54 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QPIRI", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QFLAG(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QFLAG"); // 00 51 46 4C 41 47 98 74 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QFLAG", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPIGS(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QPIGS"); // 00 51 50 49 47 53 B7 A9 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QPIGSW", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMOD(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QMOD"); // 00 51 4D 4F 44 49 C1 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QMOD", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPIWS(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QPIWS"); // 00 51 50 49 57 53 B4 DA 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QPIWS", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QDI(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QDI"); // 00 51 44 49 71 1B 0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QDI", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMCHGCR(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QMCHGCR"); // 0x00, 0x51, 0x4D, 0x43, 0x48, 0x47, 0x43, 0x52, 0xD8, 0x55, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\tReply: {reponseData}", "QMCHGCR", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMUCHGCR(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QMUCHGCR"); // 0x00, 0x51, 0x4D, 0x55, 0x43, 0x48, 0x47, 0x43, 0x52, 0x26, 0x34, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\tReply: {reponseData}", "QMUCHGCR", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QOPPT(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QOPPT"); // 00, 0x51, 0x4F, 0x50, 0x50, 0x54, 0x4F, 0x11, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QOPPT", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QCHPT(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QCHPT"); // 00, 0x51, 0x43, 0x48, 0x50, 0x54, 0xEA, 0xE1, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QCHPT", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QT(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QT"); // 00, 0x51, 0x54, 0x27, 0xFF, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QT", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMN(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QMN"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QMN", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QGMN(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QGMN"); // 00, 0x51, 0x47, 0x4D, 0x4E, 0x49, 0x29, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QGMN", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QBEQI(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QBEQI"); // 00, 0x51, 0x42, 0x45, 0x51, 0x49, 0x2E, 0xA9, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QBEQI", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QET(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QET"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QET", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QEY(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QEY{date.Value.Year:0000}";
            var request = this._communicationsClient.GenerateGetRequest(command);

            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\tReply: {reponseData}", command, System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QEM(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QEM{date.Value.Year:0000}{date.Value.Month:00}";
            var request = this._communicationsClient.GenerateGetRequest(command);

            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\tReply: {reponseData}", command, System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QED(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QED{date.Value.Year:0000}{date.Value.Month:00}{date.Value.Day:00}";
            var request = this._communicationsClient.GenerateGetRequest(command);

            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\tReply: {reponseData}", command, System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLT(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QLT"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\tReply: {reponseData}", "QLT", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLY(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QLY{date.Value.Year:0000}";
            var request = this._communicationsClient.GenerateGetRequest(command);

            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\tReply: {reponseData}", command, System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLM(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QLM{date.Value.Year:0000}{date.Value.Month:00}";
            var request = this._communicationsClient.GenerateGetRequest(command);

            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\tReply: {reponseData}", command, System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLD(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QLD{date.Value.Year:0000}{date.Value.Month:00}{date.Value.Day:00}";
            var request = this._communicationsClient.GenerateGetRequest(command);

            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\tReply: {reponseData}", command, System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLED(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QLED"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QLED", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> Q1(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("Q1"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "Q1", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QBOOT(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QBOOT"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QBOOT", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QOPM(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QOPM"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QOPM", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPGS(byte index = 0, CancellationToken cancellationToken = default)
        {
            var command = $"QPGS{index}";
            var request = this._communicationsClient.GenerateGetRequest(command);
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", command, System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QBV(CancellationToken cancellationToken = default)
        {
            var request = this._communicationsClient.GenerateGetRequest("QBV"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await this._communicationsClient.SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", "QVB", System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> DAT(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"DAT{date.Value:yy}{date.Value.Month:00}{date.Value.Day:00}{date.Value.Hour:00}{date.Value.Minute:00}{date.Value.Second:00}";

            // ACK. Works for Serial [ttyUSB00] (no reportId, include CRC)
            // NAK, but does not crash HID [hidraw1] (include reportId, NO CRC )
            var request = this._communicationsClient.GenerateGetRequest(command);
            //var request = this._communicationsClient.GenerateGetRequest(command, includeCrc: (this._options.PortType == PortDeviceType.Serial) ? true : false);

            var response = await this._communicationsClient.SendRequest(request, replyExpected: true);
            if (response.IsSuccess)
            {
                this._logger.LogDebug("Request: {commandCode}\t\tReply: {reponseData}", command, System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

    }
}
