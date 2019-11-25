using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class FrameMetaDataConfiguration
    {
        public int MetaDataLength { get; set; }

        [Required(ErrorMessage = "Specify sender ID length.")]
        [Range(0, Int32.MaxValue)]
        public int SenderIdLength { get; set; }

        [Required(ErrorMessage = "Specify headers data length.")]
        [Range(0, Int32.MaxValue)]
        public int HeadersDataLength { get; set; }

        [Required(ErrorMessage = "Specify message data length.")]
        [Range(0, Int32.MaxValue)]
        public int MessageDataLength { get; set; }
    }
}
