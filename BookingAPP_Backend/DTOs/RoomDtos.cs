using System.ComponentModel.DataAnnotations;

namespace BookingAPP_Backend.DTOs
{
    public class ServiceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Cost { get; set; }
    }

    public class RoomResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal BaseHourlyRate { get; set; }
        public bool IsActive { get; set; }
        public List<ServiceDto> AvailableServices { get; set; } = new();
    }

    // запит на створення нового залу
    public class CreateRoomRequest
    {
        [Required(ErrorMessage = "Назва залу є обов'язковою.")]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 10000, ErrorMessage = "Місткість має бути більшою за 0.")]
        public int Capacity { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Базова вартість оренди має бути додатною.")]
        public decimal BaseHourlyRate { get; set; }

        // cписок ідентифікаторів існуючих послуг доступних у цьому залі
        public List<Guid>? ServiceIds { get; set; }

        // нові послуги яких ще немає в каталозі будуть створені разом із залом
        public List<CreateServiceRequest>? NewServices { get; set; }
    }

    // запит на оновлення існуючого залу 
    public class UpdateRoomRequest
    {
        [StringLength(200, MinimumLength = 2)]
        public string? Name { get; set; }

        [Range(1, 10000)]
        public int? Capacity { get; set; }

        [Range(0.01, 1000000)]
        public decimal? BaseHourlyRate { get; set; }

        public bool? IsActive { get; set; }

        public List<Guid>? ServiceIds { get; set; }

        // нові послуги для додавання в каталог і одразу прив'язки до залу 
        public List<CreateServiceRequest>? NewServices { get; set; }
    }

    public class CreateServiceRequest
    {
        [Required, StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal Cost { get; set; }
    }

    // запит на пошук доступних залів
    public class RoomSearchRequest
    {
        [Required(ErrorMessage = "Дата бронювання є обов'язковою.")]
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Час початку є обов'язковим.")]
        public TimeOnly TimeFrom { get; set; }

        [Required(ErrorMessage = "Час завершення є обов'язковим.")]
        public TimeOnly TimeTo { get; set; }

        [Range(1, 10000)]
        public int? MinCapacity { get; set; }
    }

}
