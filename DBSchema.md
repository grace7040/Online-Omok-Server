# 유저의 계정 데이터
```mermaid
erDiagram
    user_account_data{
        INT uid PK
        VARCHAR(50) email UK
        VARCHAR(100) password
    }
```
---
# 유저의 인게임 데이터
```mermaid
erDiagram
    user_game_data{
        INT uid PK
        VARCHAR(50) email UK
        INT level
        INT exp
        INT win_count
        INT lose_count
    }
```