namespace HVO.PowerMonitor.V1.HostedServices.VoltronicInverterService
{
    public partial class VoltronicInverter
    {
        private byte[] GenerateRequest(string commandCode, bool includeCrc = true)
        {
            var commandBytes = System.Text.Encoding.ASCII.GetBytes(commandCode);

            // Generate the request
            List<byte> request = new List<byte>();
            request.AddRange(commandBytes);

            if (includeCrc)
            {
                // Get the CRC for this payload
                var crc = CalculateCrc(commandBytes, 0);
                request.AddRange(crc);
            }

            request.Add(0x0D);

            var response = request.ToArray();
            //this._logger.LogDebug("CommandCode: {commandCode}, IncludeCRC: {includeCrc} -- Request: {request}", commandCode, includeCrc, BitConverterExtras.BytesToHexString(response));

            return response;
        }


        public async Task<(bool IsSuccess, string ProtocolID)> QPI(CancellationToken cancellationToken = default)
        {
            var request = $"QPI";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, string SerialNumber)> QID(CancellationToken cancellationToken = default)
        {
            var request = $"QID";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, string SerialNumber)> QSID(CancellationToken cancellationToken = default)
        {
            var request = $"QSID";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, string Version)> QVFW(CancellationToken cancellationToken = default)
        {
            var request = $"QVFW";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, string Version)> QVFW2(CancellationToken cancellationToken = default)
        {
            var request = $"QVFW2";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, string Version)> QVFW3(CancellationToken cancellationToken = default)
        {
            var request = $"QVFW3";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, string Version)> VERFW(CancellationToken cancellationToken = default)
        {
            var request = $"VERFW";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPIRI(CancellationToken cancellationToken = default)
        {
            var request = $"QPIRI";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QFLAG(CancellationToken cancellationToken = default)
        {
            var request = $"QFLAG";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPIGS(CancellationToken cancellationToken = default)
        {
            var request = $"QPIGS";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMOD(CancellationToken cancellationToken = default)
        {
            var request = $"QMOD";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPIWS(CancellationToken cancellationToken = default)
        {
            var request = $"QPIWS";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QDI(CancellationToken cancellationToken = default)
        {
            var request = $"QDI";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMCHGCR(CancellationToken cancellationToken = default)
        {
            var request = $"QMCHGCR";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMUCHGCR(CancellationToken cancellationToken = default)
        {
            var request = $"QMUCHGCR";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QOPPT(CancellationToken cancellationToken = default)
        {
            var request = $"QOPPT";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QCHPT(CancellationToken cancellationToken = default)
        {
            var request = $"QCHPT";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QT(CancellationToken cancellationToken = default)
        {
            var request = $"QT";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMN(CancellationToken cancellationToken = default)
        {
            var request = $"QMN";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QGMN(CancellationToken cancellationToken = default)
        {
            var request = $"QGMN";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QBEQI(CancellationToken cancellationToken = default)
        {
            var request = $"QBEQI";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QET(CancellationToken cancellationToken = default)
        {
            var request = $"QET";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QEY(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var request = $"QEY{date.Value.Year:0000}";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QEM(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var request = $"QEM{date.Value.Year:0000}{date.Value.Month:00}";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QED(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var request = $"QED{date.Value.Year:0000}{date.Value.Month:00}{date.Value.Day:00}";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLT(CancellationToken cancellationToken = default)
        {
            var request = $"QLT";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLY(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var request = $"QLY{date.Value.Year:0000}";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLM(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var request = $"QLM{date.Value.Year:0000}{date.Value.Month:00}";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLD(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var request = $"QLD{date.Value.Year:0000}{date.Value.Month:00}{date.Value.Day:00}";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLED(CancellationToken cancellationToken = default)
        {
            var request = $"QLED";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> Q1(CancellationToken cancellationToken = default)
        {
            var request = $"Q1";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QBOOT(CancellationToken cancellationToken = default)
        {
            var request = $"QBOOT";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QOPM(CancellationToken cancellationToken = default)
        {
            var request = $"QOPM";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPGS(byte index = 0, CancellationToken cancellationToken = default)
        {
            var request = $"QPGS{index}";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QBV(CancellationToken cancellationToken = default)
        {
            var request = $"QBV";
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> DAT(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var request = $"DAT{date.Value:yy}{date.Value.Month:00}{date.Value.Day:00}{date.Value.Hour:00}{date.Value.Minute:00}{date.Value.Second:00}";

            // ACK. Works for Serial [ttyUSB00] (no reportId, include CRC)
            // NAK, but does not crash HID [hidraw1] (include reportId, NO CRC )
            var response = await this.SendRequest(this.GenerateRequest(request), cancellationToken: cancellationToken);

            if (response.IsSuccess)
            {
                return (true, null);
            }

            return (false, null);
        }
    }
}
