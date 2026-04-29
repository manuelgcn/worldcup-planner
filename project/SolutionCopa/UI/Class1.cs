namespace UI;

public class Class1
{
    
 // Cuando queramos utilizar servicios tenemos que hacer 
 // builder.Services.AddSingleton<IMatchService, MatchService>();
 // Indica q usamos IMatchServices usando la implementacion de MatchService
 // Existe solo 1 a lo largo del ciclo de vida de la app. Util en primera entrega
 
 //builder.Services.AddScoped<IMatchService, MatchService>();
 //Mas eficiente de recursos, se crea una instancia por cada request, se destruye al finalizar la request. Util en segunda entrega
 //
 
 //builder.Services.AddTransient<IMatchService, MatchService>();
 // Vive menos tiempo que el Scoped, se crea una instancia cada vez que se inyecta, se destruye al finalizar la request
}