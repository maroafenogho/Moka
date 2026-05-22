using Microsoft.AspNetCore.Mvc;

namespace Moka.src.Shared
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public T? Data { get; }

        protected internal Result(bool isSuccess, T? data, string? error)
        {
            if (isSuccess && error != null)
                throw new InvalidOperationException("Successful result cannot have an error message.");

            if (!isSuccess && data != null)
                throw new InvalidOperationException("Failed result cannot have a data.");

            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }

        public static Result<T> Success(T data) => new Result<T>(true, data, null);
        public static Result<T> Failure(string error) => new Result<T>(false, default, error);

    }
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public object? Data { get; }


        protected internal Result(bool isSuccess, object? data, string? error)
        {
            if (isSuccess && error != null)
                throw new InvalidOperationException();
            if (!isSuccess && error == null)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }

        public static Result Success() => new Result(true, null, null);
        public static Result Failure(string error) => new Result(false, null, error);
    }

    public static class ResultExtensions
    {
        public static IActionResult ToActionResult<T>(this Result<T> result)
        {
            if (result.IsSuccess)
                return new OkObjectResult(Result<T>.Success(result.Data!));

            return new BadRequestObjectResult(Result<T>.Failure(result.Error ?? "An error occurred"));
        }

        public static IActionResult ToActionResult(this Result result)
        {
            if (result.IsSuccess)
                return new OkObjectResult(Result.Success());

            return new BadRequestObjectResult(Result.Failure(result.Error ?? "An error occurred"));
        }
    }
}