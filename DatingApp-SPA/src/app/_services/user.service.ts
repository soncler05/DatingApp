import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baserUrl = environment.apiUrl + 'users/';
  constructor(private http: HttpClient) {}

  getUsers(page?, itemsPerPage?, userParams?): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();

    let params = new HttpParams();

    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }
    if (userParams) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }

    return this.http.get<User[]>(this.baserUrl.substring(0, this.baserUrl.length - 1),
                                                        {observe: 'response', params}).
    pipe(
      map(response => {
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') != null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    );
  }

  getUser(id: number): Observable<User> {
    return this.http.get<User>(this.baserUrl + id);
  }

  UpdateUser(id: number, user: User): Observable<User> {
    return this.http.put<User>(this.baserUrl + id, user);
  }

  setUserPhotoToMain(id: number, userId: number) {
      return this.http.post(this.baserUrl + userId + '/photos/' + id + '/SetMain', null);
  }

  deletePhoto(userId: number, id: number ) {
    return this.http.delete(this.baserUrl + userId + '/photos/' + id);
  }
}
