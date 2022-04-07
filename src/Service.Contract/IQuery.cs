namespace WebApp.Service;

public interface IQuery { }

public interface IQuery<out TResult> : IQuery { }
