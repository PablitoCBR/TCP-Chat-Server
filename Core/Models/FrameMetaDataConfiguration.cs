using System;
using System.ComponentModel.DataAnnotations;

namespace Core.Models
{
    public class FrameMetaDataConfiguration
    {
        public int MetaDataFieldsTotalSize { get; set; }

        [Required(ErrorMessage = "Specify headers data length.")]
        [Range(0, Int32.MaxValue)]
        public int HeadersLengthFieldSize { get; set; }

        [Required(ErrorMessage = "Specify message data length.")]
        [Range(0, Int32.MaxValue)]
        public int MessageLengthFieldSize { get; set; }
    }
}
