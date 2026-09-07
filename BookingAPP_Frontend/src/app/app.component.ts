import { Component, OnInit } from '@angular/core';
import { ApiService, RoomAvailability, Booking } from './api.service';

type SortColumn = 'name' | 'capacity' | 'baseHourlyRate' | 'isAvailable';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'Оренда конференц-залів';

  // --- Параметри періоду (впливають на статус "вільний / заброньований") ---
  date = new Date().toISOString().slice(0, 10);
  timeFrom = '10:00';
  timeTo = '14:00';
  minCapacity: number | null = null;

  // --- Список УСІХ залів зі статусом ---
  loadingRooms = false;
  rooms: RoomAvailability[] = [];
  roomsError = '';

  // --- Сортування таблиці ---
  sortColumn: SortColumn = 'name';
  sortAsc = true;

  // --- Форма бронювання ---
  selectedRoom: RoomAvailability | null = null;
  customerName = '';
  customerEmail = '';
  selectedServiceIds = new Set<string>();
  booking = false;
  bookingError = '';
  lastBookingResult: Booking | null = null;

  // --- Історія всіх бронювань ---
  allBookings: Booking[] = [];
  loadingBookings = false;
  bookingsError = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadRooms();
    this.loadBookings();
  }

  // ---------- Список усіх залів (заброньовано / вільно) ----------

  loadRooms(): void {
    this.loadingRooms = true;
    this.roomsError = '';
    this.lastBookingResult = null;

    this.api.getRoomsAvailability(this.date, this.timeFrom, this.timeTo, this.minCapacity).subscribe({
      next: rooms => {
        this.rooms = rooms;
        this.loadingRooms = false;
        this.applySort();
      },
      error: err => {
        this.loadingRooms = false;
        this.roomsError = err.error?.message ?? `Помилка запиту (${err.status}). Перевірте, чи запущено бекенд.`;
      }
    });
  }

  // ---------- Сортування ----------

  sortBy(column: SortColumn): void {
    if (this.sortColumn === column) {
      this.sortAsc = !this.sortAsc;
    } else {
      this.sortColumn = column;
      this.sortAsc = true;
    }
    this.applySort();
  }

  sortArrow(column: SortColumn): string {
    if (this.sortColumn !== column) return '';
    return this.sortAsc ? '▲' : '▼';
  }

  private applySort(): void {
    const dir = this.sortAsc ? 1 : -1;
    const col = this.sortColumn;

    this.rooms = [...this.rooms].sort((a, b) => {
      let av: number | string = a[col] as any;
      let bv: number | string = b[col] as any;

      if (col === 'isAvailable') {
        // Вільні зали — першими при сортуванні за зростанням
        av = a.isAvailable ? 0 : 1;
        bv = b.isAvailable ? 0 : 1;
      }

      if (typeof av === 'string' && typeof bv === 'string') {
        return av.localeCompare(bv) * dir;
      }
      return ((av as number) - (bv as number)) * dir;
    });
  }

  // ---------- Бронювання ----------

  selectRoom(room: RoomAvailability): void {
    if (!room.isAvailable) return;
    this.selectedRoom = room;
    this.customerName = '';
    this.customerEmail = '';
    this.selectedServiceIds.clear();
    this.lastBookingResult = null;
    this.bookingError = '';
  }

  cancelSelection(): void {
    this.selectedRoom = null;
  }

  toggleService(id: string): void {
    if (this.selectedServiceIds.has(id)) this.selectedServiceIds.delete(id);
    else this.selectedServiceIds.add(id);
  }

  isServiceSelected(id: string): boolean {
    return this.selectedServiceIds.has(id);
  }

  book(): void {
    if (!this.selectedRoom) return;

    if (!this.customerName.trim() || !this.customerEmail.trim()) {
      this.bookingError = "Вкажіть ім'я та email.";
      return;
    }

    this.booking = true;
    this.bookingError = '';

    this.api.createBooking({
      roomId: this.selectedRoom.id,
      customerName: this.customerName.trim(),
      customerEmail: this.customerEmail.trim(),
      date: this.date,
      timeFrom: this.timeFrom,
      timeTo: this.timeTo,
      serviceIds: Array.from(this.selectedServiceIds)
    }).subscribe({
      next: result => {
        this.booking = false;
        this.lastBookingResult = result;
        this.selectedRoom = null;
        this.loadRooms();      // статус залів оновиться (щойно заброньований стане "Заброньовано")
        this.loadBookings();   // оновити історію бронювань
      },
      error: err => {
        this.booking = false;
        this.bookingError = err.error?.message ?? `Не вдалося створити бронювання (${err.status}).`;
      }
    });
  }

  // ---------- Історія бронювань ----------

  loadBookings(): void {
    this.loadingBookings = true;
    this.bookingsError = '';

    this.api.getBookings().subscribe({
      next: bookings => {
        this.allBookings = bookings;
        this.loadingBookings = false;
      },
      error: err => {
        this.loadingBookings = false;
        this.bookingsError = err.error?.message ?? `Не вдалося завантажити бронювання (${err.status}).`;
      }
    });
  }

  cancelBooking(b: Booking): void {
    if (!confirm(`Скасувати бронювання "${b.roomName}"?`)) return;

    this.api.cancelBooking(b.id).subscribe({
      next: () => {
        this.loadBookings();
        this.loadRooms(); // зал може знову стати вільним
      },
      error: err => (this.bookingsError = err.error?.message ?? 'Не вдалося скасувати бронювання.')
    });
  }
}
