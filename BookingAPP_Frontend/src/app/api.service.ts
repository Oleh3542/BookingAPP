import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

// ЗМІНІТЬ ТУТ, якщо бекенд запускається на іншому порту
// (порт дивіться в BookingAPP_Backend/Properties/launchSettings.json -> applicationUrl, профіль https)
const API_BASE = 'https://localhost:7178/api/v1';

export interface Service {
  id: string;
  name: string;
  cost: number;
}

export interface RoomAvailability {
  id: string;
  name: string;
  capacity: number;
  baseHourlyRate: number;
  isActive: boolean;
  isAvailable: boolean;
  availableServices: Service[];
}

export interface Booking {
  id: string;
  roomId: string;
  roomName: string;
  customerName: string;
  customerEmail: string;
  startTime: string;
  endTime: string;
  roomCost: number;
  servicesCost: number;
  totalCost: number;
  status: string; // "Confirmed" | "Cancelled"
  services: { serviceId: string; serviceName: string; price: number }[];
  priceBreakdown: string[];
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  /** Усі зали на обраний період одразу з позначкою isAvailable — для загальної таблиці. */
  getRoomsAvailability(date: string, timeFrom: string, timeTo: string, minCapacity: number | null): Observable<RoomAvailability[]> {
    let params = new HttpParams().set('date', date).set('timeFrom', timeFrom).set('timeTo', timeTo);
    if (minCapacity) params = params.set('minCapacity', minCapacity);
    return this.http.get<RoomAvailability[]>(`${API_BASE}/rooms/availability`, { params });
  }

  createBooking(data: {
    roomId: string;
    customerName: string;
    customerEmail: string;
    date: string;
    timeFrom: string;
    timeTo: string;
    serviceIds: string[];
  }): Observable<Booking> {
    return this.http.post<Booking>(`${API_BASE}/bookings`, data);
  }

  getBookings(): Observable<Booking[]> {
    return this.http.get<Booking[]>(`${API_BASE}/bookings`);
  }

  cancelBooking(id: string): Observable<void> {
    return this.http.post<void>(`${API_BASE}/bookings/${id}/cancel`, {});
  }
}
