namespace IndieVault.DTOs
{
    // The first result set every read stored procedure returns. ResponseCode mirrors HTTP status
    // codes (200 success, 500 unexpected error) so a repository only needs to understand one scheme
    // to log and react to what a stored procedure did.
    public class StoredProcedureResponseDto
    {
        public int ResponseCode { get; set; }
        public string ResponseMessage { get; set; } = string.Empty;

        public bool IsSuccess => ResponseCode is >= 200 and < 300;
    }
}
