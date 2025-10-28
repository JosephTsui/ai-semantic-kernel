import os
import json
from typing import List
from dataclasses import dataclass, asdict
from semantic_kernel.functions.kernel_function_decorator import kernel_function

@dataclass
class FlightModel:
    Id: int
    Airline: str
    Destination: str
    DepartureDate: str
    Price: float
    IsBooked: bool = False

class FlightBookingPlugin:
    FILE_PATH = "flights.json"

    def __init__(self):
        self.flights: List[FlightModel] = self.load_flights_from_file()

    @kernel_function(description="Searches for available flights based on the destination and departure date in the format YYYY-MM-DD")
    def search_flights(self, destination, departure_date):
        matching_flights = [
            flight for flight in self.flights
            if flight.Destination.lower() == destination.lower() and flight.DepartureDate == departure_date
        ]
        return matching_flights

    @kernel_function(description="Books a flight based on the flight ID provided")
    def book_flight(self, flight_id):
        flight = next((f for f in self.flights if f.Id == flight_id), None)
    
        if flight is None:
            return "Flight not found. Please provide a valid flight ID."

        if flight.IsBooked:
            return "You've already booked this flight."

        flight.IsBooked = True
        self.save_flights_to_file()

        return (
            f"Flight booked successfully! Airline: {flight.Airline}, "
            f"Destination: {flight.Destination}, Departure: {flight.DepartureDate}, "
            f"Price: ${flight.Price}."
        )

    def save_flights_to_file(self):
        with open(self.FILE_PATH, 'w', encoding='utf-8') as f:
            json.dump([asdict(flight) for flight in self.flights], f, indent=4)

    def load_flights_from_file(self) -> List[FlightModel]:
        if os.path.exists(self.FILE_PATH):
            with open(self.FILE_PATH, 'r', encoding='utf-8') as f:
                data = json.load(f)
                return [FlightModel(**item) for item in data]

        raise FileNotFoundError(f"The file '{self.FILE_PATH}' was not found. Please provide a valid flights.json file.")
