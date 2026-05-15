using System;
using System.Collections.Generic;

namespace BenhVien_API.Models;

public partial class Messagedetail
{
    public int ChatId { get; set; }

    public int MessageId { get; set; }

    public string? ImageUrl { get; set; }

    public int? SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string? MessageText { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Chatwithdoctor Chat { get; set; } = null!;
}
