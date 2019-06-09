namespace EasyProxy.Server.Dtos
{
    public class ResultDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }
    }

    public class ResultDto<T> : ResultDto
    {
        public ResultDto(T data)
        {
            Data = data;
        }

        public T Data { get; set; }
    }
}
