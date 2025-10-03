// types.ts
export enum VoiceType {
  Tenor = 'Tenor',
  Baritone = 'Baritone',
  Bass = 'Bass',
  Soprano = 'Soprano',
  MezzoSoprano = 'MezzoSoprano',
  Contralto = 'Contralto'
}

export enum Gender {
  Male = 'Male',
  Female = 'Female'
}

export enum AgeRestriction {
  AllAges = 'AllAges',
  SixPlus = 'SixPlus',
  TwelvePlus = 'TwelvePlus',
  SixteenPlus = 'SixteenPlus',
  EighteenPlus = 'EighteenPlus'
}

export enum RoleType {
  Main = 'Main',
  Supporting = 'Supporting',
  Ensemble = 'Ensemble'
}

export const voiceTypeTranslations: Record<VoiceType, string> = {
  [VoiceType.Tenor]: 'Тенор',
  [VoiceType.Baritone]: 'Баритон',
  [VoiceType.Bass]: 'Бас',
  [VoiceType.Soprano]: 'Сопрано',
  [VoiceType.MezzoSoprano]: 'Меццо-сопрано',
  [VoiceType.Contralto]: 'Контральто'
};

export const genderTranslations: Record<Gender, string> = {
  [Gender.Male]: 'Муж.',
  [Gender.Female]: 'Жен.'
};

export const ageRestrictionTranslations: Record<AgeRestriction, string> = {
  [AgeRestriction.AllAges]: '0+',
  [AgeRestriction.SixPlus]: '6+',
  [AgeRestriction.TwelvePlus]: '12+',
  [AgeRestriction.SixteenPlus]: '16+',
  [AgeRestriction.EighteenPlus]: '18+'
};

export const roleTypeTranslations: Record<RoleType, string> = {
  [RoleType.Main]: 'Главная',
  [RoleType.Supporting]: 'Второстепенная',
  [RoleType.Ensemble]: 'Ансамбль'
};



export const voiceTypeOptions = Object.values(VoiceType).map(value => ({
  value,
  label: voiceTypeTranslations[value]
}));

export const genderOptions = Object.values(Gender).map(value => ({
  value,
  label: genderTranslations[value]
}));

export const ageRestrictionOptions = Object.values(AgeRestriction).map(value => ({
  value,
  label: ageRestrictionTranslations[value]
}));

export const roleTypeOptions = Object.values(RoleType).map(value => ({
  value,
  label: roleTypeTranslations[value]
}));

export interface ActorDTO {
  id: number;
  name: string;
  voiceType: VoiceType;
  gender: Gender;
  birthDate: string;
  height: number;
  weight: number;
  addInfo?: string;
}

export interface MusicalDTO {
  id: number;
  title: string;
  description: string;
  duration: string;
  ageRestriction: AgeRestriction;
  theatreId: number;
}

export interface TheatreDTO {
  id: number;
  name: string;
  addInfo?: string;
}

export interface RoleDTO {
  id: number;
  name: string;
  musicalId: number;
  roleType: RoleType;
}

export interface ShowDTO {
  id: number;
  date: string;
  musicalId: number;
}

export interface ThemeDTO {
  id: number;
  name: string;
}

export interface CastMemberDTO {
  id: number;
  showId: number;
  roleId: number;
  actorId: number;
  comment?: string;
}

export interface FavoriteItemDTO {
  id: number;
  name: string;
  lastModified: string;
}

export interface AccountFavoritesDTO {
  favoriteActors: FavoriteItemDTO[];
  favoriteMusicals: FavoriteItemDTO[];
  favoriteTheatres: FavoriteItemDTO[];
}

export interface AddFavoriteDTO {
  accountId: number;
  targetId: number;
}

export interface RemoveFavoriteDTO {
  accountId: number;
  targetId: number;
}


export enum AccessLevel {
  Admin = 'Admin',
  User = 'User'
}

export const accessLevelTranslations: Record<AccessLevel, string> = {
  [AccessLevel.Admin]: 'Администратор',
  [AccessLevel.User]: 'Пользователь'
};

export interface AuthDTO {
  username: string;
  passwordHash: string;
}

export interface AccountDTO {
  id: number;
  username: string;
  LastFavoritesViewDate: string;
  accessLevel: AccessLevel;
  upgradeRequest: boolean;
}

export interface AuthResponse {
  token: string;
  account: AccountDTO;
}
