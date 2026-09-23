namespace TechGearAuction.Domain.Enums;

public enum UserStatus { Active, Banned, Closed }
public enum AuctionStatus { Draft, Scheduled, Active, Completed, Cancelled }
public enum ReportType { Flake, Scam, Harassment }
public enum ReportStatus { Pending, Investigating, Resolved, Dismissed, Rejected }
public enum AppealStatus { Pending, Approved, Rejected }
public enum MediaType { Image, Video }
public enum UserRole { User, Moderator, Admin }
public enum ChatRoomStatus { Active, Archived }
public enum ChatMessageType { Text, Image, Video, SystemText }
public enum ChatRoomRole { All, Buyer, Seller }
public enum ChatRoomFolder { Inbox, Archived }