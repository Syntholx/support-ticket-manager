public class SampleTicketData
{
    public List<Ticket> CreateSampleTickets()
    {
        Ticket firstTicket = new Ticket(
     id: 1,
     title: "Problem z logowaniem",
     description: "Użytkownik nie może zalogować się do panelu ",
     priority: 4,
      status: "Open"
    );

        Ticket secondTicket = new Ticket(
            2,
            "Błąd płatności",
            "Płatność została pobrana, ale zamówienie nie powstało",
            5,
            "Closed"
        );

        Ticket thirdTicket = new Ticket(
            3,
            "Pytanie o fakturę",
            "Użytkownik prosi o kopię faktury",
            2,
            "InProgress"
        );
        Ticket fourthTicket = new Ticket(
            4,
            "Reset hasła zakończony",
           "Użytkownik odzyskał dostęp do konta",
            3,
            "Closed"
        );
        Ticket fifthTicket = new Ticket(
        id: 5,
        title: "Problem z adresem dostawy",
        description: "Użytkownik chce poprawić adres przed wysyłką",
        priority: 1,
        status: "Open"
        );


        List<Ticket> sampleTickets = new List<Ticket>
    {
    firstTicket,
    secondTicket,
    thirdTicket,
    fourthTicket,
    fifthTicket
    };
        return sampleTickets;
    }

}