<?php

use Illuminate\Database\Schema\Blueprint;
use Illuminate\Database\Migrations\Migration;

class CreateTextsTable extends Migration {

    /**
     * Run the migrations.
     *
     * @return void
     */
    public function up() {
        Schema::create('texts', function(Blueprint $table) {
            $table->increments('id');
            $table->string('title', 100);
            $table->string('collectionName', 100);
            $table->integer('collectionNo')->nullable();
            $table->integer('l1_id')->unsigned();
            $table->integer('l2_id')->unsigned()->nullable();
            $table->timestamp('created_at');
            $table->timestamp('updated_at');
            $table->timestamp('lastRead')->nullable();
            $table->integer('user_id')->unsigned();
            $table->text('l1Text');
            $table->text('l2Text');
            $table->string('audioUrl', 250);
            $table->boolean('shareAudioUrl');
            
            $table->foreign('user_id')->references('id')->on('users');
            $table->foreign('l1_id')->references('id')->on('languages');
        });
    }

    /**
     * Reverse the migrations.
     *
     * @return void
     */
    public function down() {
        Schema::drop('texts');
    }

}
