# 유저의 로그인
```mermaid
sequenceDiagram
    actor User
    participant GameServer
    participant HiveServer
    participant GameDB

    User->>+HiveServer: email과 pw을 통해 로그인 요청
    HiveServer->>HiveServer: 토큰을 생성하여 Redis에 저장
    HiveServer-->>-User: 토큰 전달
    User->>+GameServer: email과 토큰을 통해 로그인 요청
    GameServer->>+HiveServer: email과 토큰의 유효성 검증 요청
    HiveServer->>HiveServer: Redis에 검증 요청
    HiveServer-->>-GameServer: 유효성 검증 결과
    alt 검증 실패
        GameServer-->>User: 로그인 실패 응답
    end
    GameServer->>GameServer: 토큰을 Redis에 저장
    alt 최초 로그인
        GameServer->>+GameDB: 유저의 인게임 데이터 생성 요청
    end
    GameServer-->>-User: 로그인 성공 응답
```
---
# 새로운 유저의 계정 생성
```mermaid
sequenceDiagram
    actor User
    participant HiveServer
    participant HiveDB

    User->>+HiveServer: email과 pw를 통해 가입 요청
    HiveServer->>+HiveDB: email을 통해 데이터 조회
    HiveDB-->>-HiveServer: 데이터 조회 결과
    alt 이미 계정 존재
        HiveServer-->>User: 계정 생성 실패 응답
    end
    HiveServer->>+HiveDB: 유저의 계정 데이터 생성 요청
    HiveServer-->>-User: 계정 생성 성공 응답
```

---
# HTTP 요청 시마다 유저 인증
```mermaid
sequenceDiagram
    actor User
    participant GameServer

    User->>+GameServer: HTTP 요청 (email 및 토큰 포함)
    GameServer->>GameServer: Redis에 검증 요청
    alt 검증 성공
        GameServer-->>User: HTTP 요청 처리 후 응답
    else 검증 실패
        GameServer-->>-User: 유저 인증 실패 응답
    end
```

---
# 유저 매칭
```mermaid
sequenceDiagram
    actor User
    participant GameServer
    participant MatchingServer
    participant OmokServer

    loop Every Second && Not Matched
        User-->>+GameServer: email을 통해 매칭 요청
        GameServer-->>+MatchingServer: email 및 매칭 요청 전달
        alt 매칭 완료 정보 존재
            MatchingServer-->>-GameServer: 오목서버 주소 및 방 번호 전달
            GameServer-->>-User: 오목서버 주소 및 방 번호 응답
        else else
            opt 매칭 대기 유저수 > 2
                MatchingServer->>MatchingServer: redis request list에 요청 push
                
            end
        end
    end

    loop Every Second
        OmokServer->>OmokServer: redis request list 확인
        opt request가 존재
            OmokServer->>OmokServer: 해당 request를 pop
            OmokServer->>OmokServer: redis complete list에 자신의 서버 주소 및 빈 방 정보 응답 push
        end
    end

    loop Every Second
        MatchingServer->>MatchingServer: redis complete list 확인
        opt response가 존재
            MatchingServer->>MatchingServer: 해당 response를 pop
            MatchingServer->>MatchingServer: 매칭 완료 정보 저장
        end
    end    
```