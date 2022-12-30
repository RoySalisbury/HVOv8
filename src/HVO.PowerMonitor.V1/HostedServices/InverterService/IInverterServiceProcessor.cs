namespace HVO.PowerMonitor.V1.HostedServices.InverterService
{
    public interface IInverterServiceProcessor 
    {
        Task<(bool IsSuccess, string ProtocolID)> QPI(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, string SerialNumber)> QID(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, string SerialNumber)> QSID(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, string Version)> QVFW(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, string Version)> QVFW2(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, string Version)> QVFW3(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, string Version)> VERFW(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QPIRI(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QFLAG(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QPIGS(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QMOD(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QPIWS(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QDI(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QMCHGCR(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QMUCHGCR(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QOPPT(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QCHPT(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QT(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QMN(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QGMN(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QBEQI(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QET(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QEY(DateTime? date = null, CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QEM(DateTime? date = null, CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QED(DateTime? date = null, CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QLT(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QLY(DateTime? date = null, CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QLM(DateTime? date = null, CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QLD(DateTime? date = null, CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QLED(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> Q1(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QBOOT(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QOPM(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QPGS(byte index = 0, CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> QBV(CancellationToken cancellationToken = default);
        Task<(bool IsSuccess, object Model)> DAT(DateTime? date = null, CancellationToken cancellationToken = default);
    }
}
