export interface User {
  id: number;
  username: string;
  firstName: string;
  lastName: string;
  email: string;
  avatarUrl?: string;
  bio?: string;
  age?: number;
  location?: string;
  interests?: string[];
  datingPreferences?: string[];
  photos?: UserPhoto[];
}

export interface UserPhoto {
  id: number;
  url: string;
  isMain: boolean;
  order: number;
}
