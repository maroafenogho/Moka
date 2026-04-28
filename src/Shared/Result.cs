using Microsoft.AspNetCore.Mvc;

namespace Moka.src.Shared
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public T? Value { get; }

        protected internal Result(bool isSuccess, T? value, string? error)
        {
            if (isSuccess && error != null)
                throw new InvalidOperationException("Successful result cannot have an error message.");

            if (!isSuccess && value != null)
                throw new InvalidOperationException("Failed result cannot have a value.");

            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);
        public static Result<T> Failure(string error) => new Result<T>(false, default, error);

    }
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public object? Value { get; }


        protected internal Result(bool isSuccess, object? value, string? error)
        {
            if (isSuccess && error != null)
                throw new InvalidOperationException();
            if (!isSuccess && error == null)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Value = value;
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
                return new OkObjectResult(Result<T>.Success(result.Value!));

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