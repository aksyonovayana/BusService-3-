using Buses.Classes;
using Laba.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Buses
{
    public class Program
    {
        static void Main(string[] args)
        {
            //FillContext();
            //return;
            using (BusContext db = new BusContext())
            {
                var Union = db.Journeys.Include(j=>j.Route).ThenInclude(r=>r.Stops).First().Route.Stops
                            .Union(db.Journeys.Include(j => j.Route).ThenInclude(r => r.Stops).First(j=>j.Id==2).Route.Stops).ToList();
                foreach (var item in Union)
                {
                    Console.WriteLine(item);
                }

                var Except = db.Journeys.Include(j => j.Route).ThenInclude(r => r.Stops).First().Route.Stops.Select(s=>s.Name)
                             .Except(db.Journeys.First(j => j.Id == 2).Route.Stops.Select(s => s.Name)).ToList();
                foreach (var item in Except)
                {
                    Console.WriteLine(item);
                }

                var Intersect = db.Journeys.Include(j => j.Route).ThenInclude(r => r.Stops).First().Route.Stops.Select(s => s.Name)
                                .Intersect(db.Journeys.First(j => j.Id == 2).Route.Stops.Select(s => s.Name)).ToList();
                foreach (var item in Intersect)
                {
                    Console.WriteLine(item);
                }

                var Join = db.Tickets.Join(db.Passengers, t => t.PassengerId, p => p.Id, (ticket, passenger) => new
                {
                    Ticket = $"{ticket}",
                    PassengerName = passenger.FullName
                }).ToList();
                foreach (var item in Join)
                {
                    Console.WriteLine($"{item.Ticket} - {item.PassengerName}");
                }

                var Distinct = db.BusStops.Select(bs => bs.Name).Distinct().ToList();
                foreach (var item in Distinct)
                {
                    Console.WriteLine(item);
                }

                var GroupBy = db.BusStops
                    .GroupBy(c => c.Name)
                    .Select(group => new
                    {
                        Name = group.Key,
                        Count = group.Count()
                    }).ToList();
                foreach (var item in GroupBy)
                {
                    Console.WriteLine($"{item.Name} - {item.Count}");
                }

                var contractCount = db.Passengers.First().Tickets.Count();
                Console.WriteLine("Number of tickets of passenger 1: " + contractCount);

                var param = new SqlParameter("@BusStopLocation", "Borshagivska 128 st.");
                var Procedure = db.Routes.FromSqlRaw("EXEC GetRoutesByBusStopLocation @BusStopLocation", param).ToList();
                foreach (var item in Procedure)
                {
                    Console.WriteLine(item);
                }

                var Function = db.Tickets.FromSqlInterpolated($"SELECT * FROM dbo.GetTicketsByPassengerId({"1"})").ToList();
                foreach (var item in Function)
                {
                    Console.WriteLine(item);
                }

                var noTracking = db.Passengers.AsNoTracking().First();
                Console.WriteLine(noTracking);
                noTracking.FullName = "NAME";
                Console.WriteLine(noTracking);

                db.SaveChanges();
            }
            using (BusContext db = new BusContext())
            {
                //eager
                var eager = db.Tickets.Include(t=>t.Passenger).First();
                Console.WriteLine(eager.Passenger.FullName);

                //explicit
                var explic = db.Passengers.First();
                db.Tickets.Where(t => t.PassengerId == explic.Id).Load();
                foreach(var ticket in explic.Tickets)
                {
                    Console.WriteLine(ticket);
                }
            }
            //lazy
            using (BusContext db = new BusContext())
            {
                var stopName = db.Journeys.First().Route.Stops.First().Name;
                Console.WriteLine(stopName);
            }
        }
        static void FillContext()
        {
            using (BusContext db = new BusContext())
            {
                var passenger = new Passenger("Yana");
                var stop1 = new BusStop("Peremogy shkola st.", "Peremogy 5 st.");
                var stop2 = new BusStop("Polyana", "Borshagivska 128 st.");
                var stop3 = new BusStop("Arkadiya", "Vadyma Hetmana 24 st.");
                var stop4 = new BusStop("Arkadiya", "Arkadii 17 st.");
                List<BusStop> busStops = new List<BusStop>()
                {
                stop1, stop2, stop3
                };
                List<BusStop> busStops2 = new List<BusStop>()
                {
                stop1, stop3, stop4
                };
                List<BusStop> busStops3 = new List<BusStop>()
                {
                stop2, stop3, stop4
                };
                Bus bus1 = new Bus(25, 10);
                Bus bus2 = new Bus(50, 20);
                MiniBus miniBus = new MiniBus(10);
                Route route = new Route(busStops);
                Route route1 = new Route(busStops2);
                Route route2 = new Route(busStops3);
                Journey journey1 = new Journey(bus1, route, new DateTime(2023, 11, 10));
                passenger.BuyTicket(journey1, "Window");
                Journey journey2 = new Journey(bus2, route1, new DateTime(2023, 12, 16));
                passenger.BuyTicket(journey2, "Front");
                Journey journey3 = new Journey(miniBus, route2, new DateTime(2023, 12, 30));
                passenger.BuyTicket(journey3, "Back");
                db.Passengers.Add(passenger);
                db.SaveChanges();
            }
        }
    }
}
