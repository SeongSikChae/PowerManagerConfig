# PowerManagerConfig

## history

### V2 v1.0.0 - dc635973
* RECONFIG 모드 시 패스워드 변경 여부를 물어봐서 패스워드를 변경할 수 있도록 변경
* V1 모드에서 MQTT 패스워드를 직접 지정 외 DAWONDNS 서버에서 패스워드를 받아와서 셋팅하는 방법 추가

### V2 v1.0.0 - 343f5a57
* 실행 파라메터 제거 (clientCertificate, clientCertificatePassword)
* 실행 파라메터 필수값 변경 (web_server_addr)
* RECONFIG 모드 추가

### V2 v1.0.0 - e5d5f965
* 디바이스 추가 시 API 인증 키 정보 자동 추가 시 Exception 발생으로 자동 추가가 되지 않는 문제 fix

### V2 v1.0.0 - 36d14843
* --api_server_addr 실행 파라메터 제거

### v1.0.1

* version parameter 요청 시 Revision 정보가 표시되게 기능을 추가하였습니다.

## 실행 파라메터

* --host 접속 하고자 하는 기기 IP (기기 WIFI 접속 시 gateway)
* --port 접속 하고자 하는 기기 PORT (현재 5000번 포트만 이용중인 것으로 파악)
* --web_server_addr 사설 관리 웹서버 URL (ex) https://10.0.0.5/

## MODE 설명

* V1 - M130, B540 (v1.0.26) 을 위한 설정 모드 (테스트 완료)
* V2 - B540 (v1.0.28) 을 위한 설정 모드 (테스트 필요, 디바이스 없음)
* V3 - B540 (v1.0.30, v1.0.32) 을 위한 설정 모드 (테스트 완료)
* V4 - B550 (v1.0.10) 을 위한 설정 모드 (테스트 완료)
* RECONFIG - DAWONDNS API 서버에서 사설 서버로 인증키를 갱신 시키는 모드
