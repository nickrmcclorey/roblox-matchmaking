public class WaitResult {
    public string? AccessCode { get; set; }
    public required string Message { get; set; }
    public ResultType Status { get; set; }


    public static WaitResult Ready(string accessCode) {
        return new WaitResult() {
            AccessCode = accessCode,
            Message = "Match found",
            Status = ResultType.Ready
        };
    }

    public static WaitResult StillWaiting() {
        return new WaitResult() {
            Message = "Matchmaking in progress",
            Status = ResultType.StillWaiting
        };
    }

    public static WaitResult BadRequest(string message) {
        return new WaitResult() {
            Message = message,
            Status = ResultType.BadRequest
        };
    }

    public static WaitResult Error(string message) {
        return new WaitResult() {
            Message = message,
            Status = ResultType.Error
        };
    }

    
    public enum ResultType {
        Ready,
        StillWaiting,
        BadRequest,
        Error
    }
}