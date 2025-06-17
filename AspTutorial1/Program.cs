using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using AspTutorial1.Exceptions;
using AspTutorial1.Formatters;
using AspTutorial1.Models;
using AspTutorial1.Models.Options;
using AspTutorial1.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Polly;

namespace AspTutorial1;

class Program
{
    
    static void Main(string[] args)
    {
 
        var builder = WebApplication.CreateBuilder(args);

        // 장기로 실행되는 백그라운드 서비스 등록할 때 사용
        //builder.Services.AddHostedService<TestBackGroundService>();
        
        #region Controller Services

        // 컨트롤러 에서 자동 HTTP 400응답을 날릴 때 해당 응답을 커스터마이징 할 수 있음
        // builder.Services.AddControllers()
        //     .ConfigureApiBehaviorOptions(options =>
        //     {
        //         // To preserve the default behavior, capture the original delegate to call later.
        //         var builtInFactory = options.InvalidModelStateResponseFactory;
        //
        //         options.InvalidModelStateResponseFactory = context =>
        //         {
        //             var logger = context.HttpContext.RequestServices
        //                 .GetRequiredService<ILogger<Program>>();
        //
        //             // Perform logging here.
        //             // ...
        //
        //             // Invoke the default behavior, which produces a ValidationProblemDetails
        //             // response.
        //             // To produce a custom response, return a different implementation of 
        //             // IActionResult instead.
        //             return builtInFactory(context);
        //         };
        //     });
        
        // 이렇게 옵션 등록도 가능
        // builder.Services.AddControllers()
        //     .ConfigureApiBehaviorOptions(options =>
        //     {
        //         options.SuppressConsumesConstraintForFormFileParameters = true;
        //         options.SuppressInferBindingSourcesForParameters = true;
        //         options.SuppressModelStateInvalidFilter = true; -> 자동 HTTP 400 응답 안하겠다는 설정
        //         options.SuppressMapClientErrors = true;
        //         options.ClientErrorMapping[StatusCodes.Status404NotFound].Link =
        //             "https://httpstatuses.com/404";
        //     });


        // 커스텀 포맷터 등록 -> json 이나, 평문 포맷터 대신 커스텀 가능
        builder.Services.AddControllers(options =>
        {
            options.InputFormatters.Insert(0, new VcardInputFormatter());
            options.OutputFormatters.Insert(0, new VcardOutputFormatter());
        });
        
        // 커스텀 포맷터 등록 -> json 이나, 평문 포맷터 대신 커스텀 가능. 이건 MemoryPack 포맷터임
        builder.Services.AddControllers(options =>
        {
            options.InputFormatters.Insert(1, new MemoryPackInputFormatter());
            options.OutputFormatters.Insert(1, new MemoryPackOutputFormatter());
        });
        
        builder.Services.AddControllers();
        
        // 에러 필터 옵션 추가
        // 컨트롤러 단에서 에러 핸들링 해주는 거임
        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<HttpResponseExceptionFilter>();
        });
        
        #endregion
        
        // Entity Framework Core 를 사용하기 위한 종속성 주입임.
        builder.Services.AddDbContext<TodoContext>(opt =>
        {
            opt.UseInMemoryDatabase("TodoList");
        });
        
        // Swagger 추가. 테스트용 UI를 사용할 수 있음 
        builder.Services.AddEndpointsApiExplorer(); // API 탐색 활성화
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tutorial API", Version = "v1" });
        });

        #region HttpClient Services

        // 기본적으로는 서버 -> API 서버 요청시 사용하는 방식임
        
        // 기본 사용법
        builder.Services.AddHttpClient();
     
        // 타입화 클라이언트
        builder.Services.AddHttpClient<TestHttpService>();
        
        // 명명된 클라이언트
        builder.Services.AddHttpClient("YouTube", client =>
        {
            client.BaseAddress = new Uri("https://www.youtube.com/watch?v=");         
        } );
        
        
        // 요청 위임 처리기. 여러개 등록 가능하고 순서대로 처리 가능.
        // 이걸 이용해서 예외 처리같은 것들을 분리하는게 가능할듯
        builder.Services.AddTransient<ValidateHeaderHandler>();
        builder.Services.AddHttpClient("DotNet", client =>
        {
            
            client.BaseAddress = new Uri("https://learn.microsoft.com/ko-kr/dotnet/api/");    
            client.DefaultRequestHeaders.Add("Test-Header", "this is test header");
            
        }).AddHttpMessageHandler<ValidateHeaderHandler>();
        
        // HttpMessageHandler 수명 설정. 기본 2분인데 5분으로
        // HttpClient는 내부적으로 HttpMessageHandler을 사용해서 통신을 함
        // 이때 HttpMessageHandler가 중요한 부분임. 이걸 내부적으로 풀링해서 사용함
        builder.Services.AddHttpClient("HandlerLifetime")
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        
        #endregion

        #region HttpClient Services - with Polly

        // Polly는 재시도, 회로 차단, 타임아웃, 폴백, 캐시 등을 지원하는 써드파티 라이브러리임.
        // asp.net은 polly를 적극 지원함
        
        // 일반적인 정책
        builder.Services.AddHttpClient("PollyWaitAndRetry", client =>
            {
                client.BaseAddress = new Uri("https://www.google.com");
            })
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(
                    3, retryNumber => TimeSpan.FromMilliseconds(600)));
        
        // 더 복잡한 정책. 분기에 따라 다른 정책 사용
        var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(10));
        var longTimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(30));

        builder.Services.AddHttpClient("PollyDynamic")
            .AddPolicyHandler(httpRequestMessage =>
                httpRequestMessage.Method == HttpMethod.Get ? timeoutPolicy : longTimeoutPolicy);

        // 여러 폴리 정책. 재시도, 써킷 브레이크
        builder.Services.AddHttpClient("PollyMultiple")
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.RetryAsync(3))
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
        

        // Polly 레지스트리로부터 정책 추가
        var policyRegistry = builder.Services.AddPolicyRegistry();

        policyRegistry.Add("Regular", timeoutPolicy);
        policyRegistry.Add("Long", longTimeoutPolicy);

        builder.Services.AddHttpClient("PollyRegistryRegular")
            .AddPolicyHandlerFromRegistry("Regular");

        builder.Services.AddHttpClient("PollyRegistryLong")
            .AddPolicyHandlerFromRegistry("Long");
        
        
        #endregion
        
        #region Options Pattern Services

        // 옵션 패턴 - 일반
        builder.Services.Configure<PositionOptions>(builder.Configuration.GetSection(PositionOptions.Position));
        
        // 옵션 패턴 - 명명된 옵션
        builder.Services.Configure<TopItemSettings>(TopItemSettings.Month,
            builder.Configuration.GetSection("TopItem:Month"));
        builder.Services.Configure<TopItemSettings>(TopItemSettings.Year,
            builder.Configuration.GetSection("TopItem:Year"));
        
        // 옵션 패턴 - 유효성 검사
        builder.Services.AddOptions<MyConfigOptions>()
            .Bind(builder.Configuration.GetSection(MyConfigOptions.MyConfig))
            .ValidateDataAnnotations()
            .Validate(config =>  // 있어도 되고 없어도 되는 부분임. 더 복잡한 유효성 검사를 하고싶을 때 사용
            {
                if (config.Key2 != 0)
                {
                    return config.Key3 > config.Key2;
                }
        
                return true;
            }, "Key3 must be > than Key2.").ValidateOnStart(); // ValidateOnStart 사용시 애플리케이션 시작시 유효성 검사

        // 옵션 패턴 - 유효성 검사 -> IValidateOptions 를 이용한 유효성 검사 코드 분리
        builder.Services.Configure<MyConfigOptions2>(builder.Configuration.GetSection(
            MyConfigOptions2.MyConfig));
        
        builder.Services.AddSingleton<IValidateOptions
            <MyConfigOptions2>, MyConfigValidation>();

        #endregion

        #region ExceptionHandler Services
        
        // 에러 핸들러 추가 -> 전역적인 에러 처리 해줌
        //builder.Services.AddExceptionHandler<CustomExceptionHandler>();
        
        // 에러 디테일 보내줄 수 있도록 함
        //builder.Services.AddProblemDetails();
        
        // 이렇게 구체적으로 에러 디테일의 옵션을 바꿀 수도 있음
        // builder.Services.AddProblemDetails(options =>
        //     options.CustomizeProblemDetails = ctx =>
        //         ctx.ProblemDetails.Extensions.Add("nodeId", Environment.MachineName));
        
        #endregion
        
        #region RateLimiter Services

                // 요청 제한기 추
        builder.Services.AddRateLimiter(options =>
        {
            
            #region RateLimiterOptions
            options.AddFixedWindowLimiter(policyName: "fixed", op => // 고정 윈도우 리미터.
                {
                    op.PermitLimit = 4; // 요청 4개
                    op.Window = TimeSpan.FromSeconds(12); // 12초마다 창 초기화
                    op.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // 가장 오래된 요청부터 처리
                    op.QueueLimit = 2; // 대기열에 들어갈 수 있는 요청 최대 2개. 나머지는 거절
                })
                .AddSlidingWindowLimiter(policyName: "sliding", op => // 슬라이딩 윈도우 리미터
                {
                    op.PermitLimit = 10; // 윈도우당 최대 10개 요청 허용
                    op.Window = TimeSpan.FromSeconds(30); // 30초 윈도우
                    op.SegmentsPerWindow = 3; // 윈도우를 3개 세그먼트로 나눔 (각 세그먼트는 10초)
                    op.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // 대기열 처리 순서
                    op.QueueLimit = 5; // 대기열 최대 5개 요청
                })
                .AddTokenBucketLimiter(policyName: "token-bucket", op => // 토큰 버킷 리미터
                {
                    op.TokenLimit = 10; // 버킷의 최대 토큰 수
                    op.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // 대기열 처리 순서
                    op.QueueLimit = 5; // 대기열 최대 크기
                    op.ReplenishmentPeriod = TimeSpan.FromSeconds(5); // 토큰 보충 주기
                    op.TokensPerPeriod = 2; // 매 주기마다 추가되는 토큰 수
                    op.AutoReplenishment = true; // 자동 토큰 보충 활성화
                })
                .AddConcurrencyLimiter(policyName: "concurrency", op => // 동시성 리미터
                {
                    op.PermitLimit = 3; // 동시에 허용되는 최대 요청 수
                    op.QueueProcessingOrder = QueueProcessingOrder.OldestFirst; // 대기열 처리 순서
                    op.QueueLimit = 5; // 대기열 최대 크기
                });
            #endregion

            #region On Rejected
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int) retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken);
            };
            #endregion
            
            #region Multiple Limiter
            // 여러 리미터를 체이닝 함. 이거 전부 만족해야함
            // _.GlobalLimiter = PartitionedRateLimiter.CreateChained(
            //     PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            //     {
            //         var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            //
            //         return RateLimitPartition.GetFixedWindowLimiter
            //         (userAgent, _ =>
            //             new FixedWindowRateLimiterOptions
            //             {
            //                 AutoReplenishment = true,
            //                 PermitLimit = 4,
            //                 Window = TimeSpan.FromSeconds(2)
            //             });
            //     }),
            //     PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            //     {
            //         var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            //
            //         return RateLimitPartition.GetFixedWindowLimiter
            //         (userAgent, _ =>
            //             new FixedWindowRateLimiterOptions
            //             {
            //                 AutoReplenishment = true,
            //                 PermitLimit = 20,    
            //                 Window = TimeSpan.FromSeconds(30)
            //             });
            //     }));
            #endregion

            #region GlobalLimiter Extend

            options.AddConcurrencyLimiter(policyName: "concurrency-extend", op =>
            {
                op.PermitLimit = 3;                   // 동시에 허용되는 최대 요청 수
                op.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;  // 대기열 처리 순서
                op.QueueLimit = 5;                    // 대기열 최대 크기
            });

            // 글로벌 동시성 제한 설정 - 엔드포인트별로 적용
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var endpoint = httpContext.Request.Path.Value?.ToLowerInvariant() ?? "";

                // 동시성이 높게 필요한 엔드포인트인 경우 별도 제한 설정
                bool isHighConcurrencyEndpoint = endpoint.Contains("/api/high-load");

                return RateLimitPartition.GetConcurrencyLimiter(
                    partitionKey: endpoint,
                    factory: _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = isHighConcurrencyEndpoint ? 10 : 5,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = isHighConcurrencyEndpoint ? 15 : 10
                    });
            });

            #endregion

        });

        #endregion

        #region Caching Services

        builder.Services.AddResponseCaching();

        #endregion
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            // 물론 json형태의 테스트만 가능함
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tutorial API v1"));
        }

        #region Error Handling Middleware

        /*
         * 기본 조건
         * 
         * 1. app.UseExceptionHandler() 단독으로는 못씀
         * 
         * 2. app.UseExceptionHandler("/Error"); 처럼 리디렉션 경로를 지정하거나, -> 이 경우 IExceptionHandler 구현체로 예외 처리 가능
         *    builder.Services.AddExceptionHandler<CustomExceptionHandler>(); 사용
         *
         * 3. app.UseExceptionHandler(exceptionHandlerApp => {...}; ); 처럼 처리 람다를 적어주거나
         *
         * 4. app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = async context => {...}; } );
         *    처럼 핸들러 지정을 해주거나
         *
         * 5. app.UseExceptionHandler(new ExceptionHandlerOptions {...}; );
         *    에 추가로 builder.Services.AddProblemDetails(); 해줘야함
         *
         */
        
        
        // 이렇게 하면 에러 발생시 /Error로 리디렉션됨
        // 최상위 미들웨어에서 동작하는 거라서, 그 이전에 에러 핸들링이 되었다면 동작 안함
        app.UseExceptionHandler("/error");

        // 이렇게만 하려면 builder.Services.AddProblemDetails(); 해야함
        // app.UseExceptionHandler(new ExceptionHandlerOptions
        // {
        //     StatusCodeSelector = ex => ex is TimeoutException
        //         ? StatusCodes.Status503ServiceUnavailable
        //         : StatusCodes.Status500InternalServerError
        // });
        
        // 단독으로 사용하려면 이렇게 ExceptionHandler 초기화 해줘야함
        // app.UseExceptionHandler(new ExceptionHandlerOptions
        // {
        //     StatusCodeSelector = ex => ex is TimeoutException
        //         ? StatusCodes.Status503ServiceUnavailable
        //         : StatusCodes.Status500InternalServerError,
        //     ExceptionHandler = async context =>
        //     {
        //         context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        //         context.Response.ContentType = MediaTypeNames.Text.Plain;
        //         await context.Response.WriteAsync("UseExceptionHandler: An exception was thrown.");
        //     }
        // });
        
        // 이렇게 하면 에러 발생시 해당 람다 실행
        // app.UseExceptionHandler(exceptionHandlerApp =>
        // {
        //     exceptionHandlerApp.Run(async context =>
        //     {
        //         context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        //
        //         // using static System.Net.Mime.MediaTypeNames;
        //         context.Response.ContentType = MediaTypeNames.Text.Plain;
        //
        //         await context.Response.WriteAsync("UseExceptionHandler: An exception was thrown.");
        //
        //         // feature를 이용해서 예외 확인 가능
        //         var exceptionHandlerPathFeature =
        //             context.Features.Get<IExceptionHandlerPathFeature>();
        //
        //         if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
        //         {
        //             await context.Response.WriteAsync("UseExceptionHandler: The file was not found.");
        //         }
        //
        //         if (exceptionHandlerPathFeature?.Path == "/")
        //         {
        //             await context.Response.WriteAsync("UseExceptionHandler: Page: Home.");
        //         }
        //     });
        // });

        #endregion

        #region Https Middleware

        app.UseHttpsRedirection();

        #endregion
        
        #region RateLimiter Middleware

        app.UseRateLimiter();

        #endregion

        #region Cahcing Middleware

        app.UseResponseCaching();
        
        // 전역 미들웨어로 모든 응답에 디폴트로 캐싱을 하겠다는 거임
        // app.Use(async (context, next) =>
        // {
        //     context.Response.GetTypedHeaders().CacheControl =
        //         new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
        //         {
        //             Public = true,
        //             MaxAge = TimeSpan.FromSeconds(10)
        //         };
        //     context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
        //         new string[] { "Accept-Encoding" };
        //
        //     await next();
        // });

        #endregion
        
        app.MapControllers();
        
        app.Run();
        
        
    }
    
}