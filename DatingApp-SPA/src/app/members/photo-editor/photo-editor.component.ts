import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { environment } from '../../../environments/environment';
import { AuthService } from 'src/app/_services/auth.service';
import { AlertifyService } from 'src/app/_services/AlertifyService.service';
import { UserService } from 'src/app/_services/user.service';
import { error } from 'protractor';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() photos: Photo[];
  @Output() getMemberPhotoChange = new EventEmitter<string>();
  baseUrl = environment.apiUrl;

  uploader: FileUploader;
  hasBaseDropZoneOver: boolean;

  constructor(private authService: AuthService, private alertify: AlertifyService,
              private userService: UserService) { }
  ngOnInit() {
    this.initializeUploader();
  }

  public fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile  = (file) => {file.withCredentials = false; };
    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        const photo = {
          id: res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          isMain: res.isMain,
          description: res.description
        };
        this.photos.push(photo);
        if (photo.isMain) {
          this.getMemberPhotoChange.emit(photo.url);
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
        }
      }
    };

  }

  setPhotoToMain(photo: Photo): void {
    this.userService.setUserPhotoToMain(photo.id, this.authService.decodedToken.nameid).subscribe( () => {
      this.setPhotoToMainClient(photo);
      this.alertify.success('The main photo has been changed successfully!!!');
    }, error => {
      this.alertify.error(error);
    });
  }

  private setPhotoToMainClient(photo: Photo) {
    const currentPhoto = this.photos.filter(p => p.isMain === true)[0];
    currentPhoto.isMain = false;
    photo.isMain = true;
    this.getMemberPhotoChange.emit(photo.url);
    this.authService.changeMemberPhoto(photo.url);
    this.authService.currentUser.photoUrl = photo.url;
    localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
  }

  deletePhoto(id: number) {
    this.alertify.confirm('Are you sure you want to delete this photo?', () => {
      this.userService.deletePhoto(this.authService.decodedToken.nameid, id).subscribe(() => {
        this.photos.splice(this.photos.findIndex(p => p.id === id), 1);
        this.alertify.success('Photo has been deleted');
      }, error => { this.alertify.error(error); });
    });
  }

}
