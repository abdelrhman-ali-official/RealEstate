using AutoMapper;
using Domain.Entities;
using Shared.ChatModels;

namespace Services.MappingProfiles
{
    public class ChatMappingProfile : Profile
    {
        public ChatMappingProfile()
        {
            CreateMap<ChatRoom, ChatRoomDto>().ReverseMap();
            CreateMap<ChatMessage, ChatMessageDto>().ReverseMap();
            CreateMap<ChatMessageReaction, ChatMessageReactionDto>().ReverseMap();
        }
    }
} 