import { Photo } from './photo';

export interface User {
    id: number;
    username: string;
    knownAs: string;
    age: number;
    created: string;
    lastActive: Date;
    photoUrl: string;
    city: string;
    country: string;
    gender: string;
    interests?: string;
    introduction?: string;
    lookingFor?: string;
    photos?: Photo[];
}
