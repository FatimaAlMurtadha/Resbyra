namespace server;

using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

/*
CREATE TABLE custom_card_hotels(
        card_id INT NOT NULL,
        hotel_id INT NOT NULL,
        PRIMARY KEY (card_id, hotel_id),
        FOREIGN KEY (card_id) REFERENCES custom_cards(id),
        FOREIGN KEY (hotel_id) REFERENCES hotels(id)
      );
*/
public class CustomCardsHotels
{

}//fatima